import { defineConfig } from 'vitepress';

// https://vitepress.dev/reference/site-config
export default defineConfig({
    title: 'Indago - Static Assembly Scanning',
    description:
        'Indago is a compile-time assembly/type-scanning library for .NET. It replaces runtime reflection-based DI scanning (like Scrutor) with a Roslyn **source generator** that resolves the scan at build time and emits a strongly-typed `IIndagoProvider`. This makes scanning AOT/trimming-friendly and removes runtime reflection.',
    themeConfig: {
        // https://vitepress.dev/reference/default-theme-config
        nav: [
            { text: 'Home', link: '/' },
            { text: 'Examples', link: '/markdown-examples' },
        ],

        sidebar: [
            {
                text: 'Examples',
                items: [
                    { text: 'Markdown Examples', link: '/markdown-examples' },
                    { text: 'Runtime API Examples', link: '/api-examples' },
                ],
            },
        ],

        socialLinks: [{ icon: 'github', link: 'https://github.com/vuejs/vitepress' }],
    },
});
