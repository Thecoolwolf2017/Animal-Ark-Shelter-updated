# Security Policy

We take security seriously. Please follow the guidance below to report vulnerabilities responsibly.

## Supported Versions

We currently support the latest release and the main branch. Older versions may not receive security updates.

## Reporting a Vulnerability

- Prefer using GitHub Security Advisories (private):
  - Go to the repository page → Security → Report a vulnerability (or open: https://github.com/Thecoolwolf2017/Animal-Ark-Shelter-updated/security/advisories/new)
  - Provide reproduction steps, impact, and environment details.
- If that is not possible, you may open a minimal public issue that omits sensitive details and reference that you have more information to share privately.

Please do NOT disclose the vulnerability publicly until we have had a chance to triage and ship a fix.

## Response Targets

- Acknowledgement: within 3 business days
- Triage: within 7 business days
- Fix and release: best-effort based on severity and complexity

## Scope Notes

- This is a single-player GTA V mod. Remote code execution risk is unlikely; however, crashes, file system writes outside `scripts/`, and data corruption are considered high impact.
- Third-party components (ScriptHookV, ScriptHookVDotNet, LemonUI) are out of scope, but we will document any mitigations/workarounds when possible.

## Coordinated Disclosure

Once a fix is available, we will:
- Publish a patched release
- Credit reporters (if desired)
- Document the issue and remediation steps

