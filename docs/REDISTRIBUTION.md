# Redistribution decision and alternative

## Current decision: hold the mixed-provenance packages

The dark baseline's creator, original distribution terms and redistribution permission remain unknown. We cannot appropriately label the inherited assets as project-owned or cleared for GitHub redistribution on the available evidence. **Do not upload the current installer, frozen payload, engineering-source ZIP, resource DLLs or extracted/derived dark artwork as a cleared release.** Existing copies remain preserved locally; no website files were changed.

This recommendation is about the absence of demonstrated permission, not a court determination that every patch is unlawful. Public availability, credit, free distribution, a different filename, re-encoding or packaging as a delta do not establish a permission grant. The current deltas reconstruct inherited resources, so changing the container does not resolve the identified provenance gap.

Microsoft's original rights are a separate layer. Microsoft's [general software terms](https://www.microsoft.com/en-us/legal/terms-of-use) direct use to the accompanying product license and restrict unauthorized redistribution. These website terms are context, not the actual Zune EULA or a substitute for a Zune-specific assessment. No Zune-specific grant authorizing this mixed asset redistribution was established here.

GitHub's [licensing guidance](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/licensing-a-repository) explains that an absent license does not grant general reuse/distribution rights. Placing material on GitHub, including a Release asset or old commit, does not supply missing rights. Project-authored code needs a deliberately scoped license if it is to be offered as open source; that license must exclude third-party content we cannot license.

## Recommended path without inherited asset redistribution

1. Prepare a **separate, reviewed code-and-documentation publication** containing only project-authored material and properly licensed dependencies. Exclude frozen payloads, bundled EXEs, Microsoft resources/compiled markup, inherited/derived dark art, and code excerpts whose rights have not been established. Review the whole proposed Git history, not just its latest tree: the present local repository has imported compiled UIX/UIB material and must not simply be pushed as the final cleared repository.
2. Document exact original-file requirements and modification steps without distributing the underlying assets. Any future local build/patch tool must take legitimately user-supplied files and contain no embedded inherited images or reconstructive deltas. This reduces redistribution of others' material; it does **not** automatically settle the user's rights to modify files or the Zune license. It has not been implemented or legally certified by this audit.
3. For a downloadable package with a dark visual option, either obtain verifiable permission from the actual third-party rights holder(s), alongside the necessary underlying rights, or replace inherited dark artwork and derivatives with independently created, appropriately licensed assets. Merely redrawing/tracing or re-inverting the inherited art is not a reliable substitute for reviewing lineage. Microsoft-dependent components still need their own assessment.
4. Until one of those paths is complete, share the attribution findings and describe the source/publication limitations honestly. Do not claim that this audit cleared RC6.2 or RC7 for redistribution.

## What this audit changed

Local README, credits, feature/provenance explanations, source coverage, licensing disclosures and publication notes now distinguish Microsoft originals, the unknown third-party dark modification, and subsequent Enhanced Zune work. Original EXEs and engineering archives were not altered or reissued. New disclosures accompany those preserved artifacts. No publishing, account action, installed-Zune change or website edit occurred.
