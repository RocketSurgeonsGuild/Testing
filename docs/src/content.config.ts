import { defineCollection } from "astro:content";
import { docsSchema } from "@astrojs/starlight/schema";
import { changelogsLoader } from "starlight-changelogs/loader";
import { starlightTagsExtension } from "starlight-tags/schema";
import { docfxApiLoader } from "./loaders/docfx-api";

export const collections = {
    docs: defineCollection({
        // Custom loader: hand-written Markdown (via Starlight's docsLoader) plus API reference
        // pages generated from `docfx metadata` output (see the `docs:api` mise task) by running
        // `docfx2astro` in-process on every load.
        loader: docfxApiLoader({
            metadataDir: ".docfx-metadata/api",
            generatedDir: ".generated/api",
            basePath: "api",
        }),
        schema: docsSchema({ extend: starlightTagsExtension }),
    }),
    changelogs: defineCollection({
        loader: changelogsLoader([
            {
                provider: "github",
                base: "changelog",
                owner: "RocketSurgeonsGuild",
                repo: "Testing",
                token: import.meta.env.GH_API_TOKEN,
            },
        ]),
    }),
};
