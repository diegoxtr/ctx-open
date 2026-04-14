# Tag-Aware Strategy

Summary:

- each cached item carries one or more tags
- updates can invalidate a full category or related entity set immediately
- implementation is slightly more complex than TTL-only

Benefit:

- keeps product reads coherent after inventory or price changes
- supports targeted invalidation instead of waiting for expiration

Use case fit:

- stronger option when freshness matters more than minimal implementation cost
