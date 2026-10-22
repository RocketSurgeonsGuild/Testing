import { execFileSync } from "node:child_process";
import { existsSync, readdirSync, readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { docsLoader } from "@astrojs/starlight/loaders";
import type { Loader, LoaderContext } from "astro/loaders";
import { parse as parseYaml } from "yaml";

/**
 * Options for {@link docfxApiLoader}.
 */
export interface DocfxApiLoaderOptions {
    /**
     * Directory (relative to the Astro project root, i.e. the `docs/` directory) containing the
     * YAML metadata produced by `docfx metadata`. See the `docs:api` mise task.
     */
    metadataDir: string;
    /**
     * Directory (relative to the Astro project root) where `docfx2astro`'s generated Markdown is
     * written. Regenerated from {@link metadataDir} on every load.
     */
    generatedDir: string;
    /** Slug prefix under which API pages live, e.g. `api`. Passed to `docfx2astro --base-slug`. */
    basePath?: string;
}

/** Recursively collects every `.md` file under `dir`. */
function collectMarkdownFiles(dir: string): string[] {
    const out: string[] = [];
    for (const entry of readdirSync(dir, { withFileTypes: true })) {
        const full = `${dir}/${entry.name}`;
        if (entry.isDirectory()) out.push(...collectMarkdownFiles(full));
        else if (entry.name.endsWith(".md")) out.push(full);
    }
    return out;
}

/** Splits a Markdown file into its YAML frontmatter (parsed) and body. */
function splitFrontmatter(raw: string): { data: Record<string, unknown>; body: string } {
    const match = raw.match(/^---\r?\n([\s\S]*?)\r?\n---\r?\n?([\s\S]*)$/);
    if (!match) return { data: {}, body: raw };
    const parsed = parseYaml(match[1]);
    return { data: (parsed ?? {}) as Record<string, unknown>, body: match[2] };
}

/**
 * A Starlight-compatible custom content loader that runs `docfx2astro` against `docfx metadata`'s
 * YAML output (see the `docs:api` mise task) and merges the resulting Markdown into the `docs`
 * collection alongside the hand-written Markdown loaded by {@link docsLoader}. Each generated page
 * already carries the Starlight frontmatter it needs (`title`, `sidebar`, `editUrl`, ...); this
 * loader derives the page id from that page's `slug` frontmatter field so routes match
 * `docfx2astro`'s own linking.
 */
export function docfxApiLoader(options: DocfxApiLoaderOptions): Loader {
    const basePath = options.basePath ?? "api";
    const base = docsLoader();

    return {
        name: "docfx-api-loader",
        async load(context: LoaderContext): Promise<void> {
            // 1. Load the hand-written Markdown docs first.
            await base.load(context);

            const metadataDirAbs = fileURLToPath(new URL(options.metadataDir, context.config.root));
            if (!existsSync(metadataDirAbs)) {
                context.logger.warn(`No docfx metadata found at ${metadataDirAbs}. Run 'mise run docs:api' to generate it before building the docs.`);
                return;
            }

            // 2. Convert that metadata into Markdown. Runs on every load so the generated pages
            //    always reflect the current docfx metadata without a separate build step.
            const generatedDirAbs = fileURLToPath(new URL(options.generatedDir, context.config.root));
            try {
                execFileSync("docfx2astro", ["-i", metadataDirAbs, "-o", generatedDirAbs, "--base-slug", basePath, "--group-by", "Namespace"], {
                    stdio: "pipe",
                });
            } catch (error) {
                const cause = error as NodeJS.ErrnoException & { stderr?: Buffer };
                if (cause.code === "ENOENT") {
                    context.logger.error("'docfx2astro' was not found on PATH. Install it via mise ('mise install') and run Astro through a mise task (e.g. 'mise run docs').");
                    return;
                }
                throw new Error(`docfx2astro failed: ${cause.stderr?.toString() ?? cause.message}`);
            }

            // 3. Read the Markdown docfx2astro just wrote and merge it into the collection.
            let count = 0;
            for (const file of collectMarkdownFiles(generatedDirAbs)) {
                const raw = readFileSync(file, "utf8");
                const { data, body } = splitFrontmatter(raw);

                // The global landing page (docfx2astro's top-level `index.md`) has no `slug` of its
                // own; it maps to the collection's base path. Every other page's `slug` frontmatter
                // (e.g. `api/dovetail.dovetailcontextbuilder`) is the source of truth for its id.
                const isRootIndex = file === `${generatedDirAbs}/index.md`;
                const rawSlug = typeof data.slug === "string" ? data.slug : undefined;
                const id = isRootIndex ? basePath : rawSlug?.replace(/^\/+/, "");
                if (!id) continue;

                // Prefer a hand-written landing page (e.g. src/content/docs/api/index.md) over the
                // auto-generated one.
                if (id === basePath && context.store.has(basePath)) continue;

                const parsedData = await context.parseData({ id, data });
                context.store.set({
                    id,
                    data: parsedData,
                    body,
                    rendered: await context.renderMarkdown(body),
                    digest: context.generateDigest(raw),
                    filePath: `src/content/docs/${id}.md`,
                });
                count++;
            }

            context.logger.info(`Loaded ${count} generated API reference page(s) from ${generatedDirAbs}.`);
        },
    };
}
