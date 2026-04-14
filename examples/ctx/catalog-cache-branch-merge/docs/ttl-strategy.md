# TTL-Only Strategy

Summary:

- each catalog key expires after a fixed time window
- no explicit invalidation hook exists
- operationally simple and cheap

Risk:

- stale price or stock can remain visible until TTL expiration
- category-level refresh cannot invalidate related product keys immediately

Use case fit:

- acceptable for low-risk read-mostly data
- weak for catalog flows where price freshness affects conversion or trust
