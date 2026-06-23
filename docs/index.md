---
# https://vitepress.dev/reference/default-theme-home-page
layout: home

hero:
    name: Indago - Static Assembly Scanning
    text: Indago is a compile-time assembly/type-scanning library for .NET. It replaces runtime reflection-based DI scanning (like Scrutor) with a Roslyn **source generator** that resolves the scan at build time and emits a strongly-typed `IIndagoProvider`. This makes scanning AOT/trimming-friendly and removes runtime reflection.
    tagline: My great project tagline
    actions:
        - theme: brand
          text: Markdown Examples
          link: /markdown-examples
        - theme: alt
          text: API Examples
          link: /api-examples

features:
    - title: Feature A
      details: Lorem ipsum dolor sit amet, consectetur adipiscing elit
    - title: Feature B
      details: Lorem ipsum dolor sit amet, consectetur adipiscing elit
    - title: Feature C
      details: Lorem ipsum dolor sit amet, consectetur adipiscing elit
---
