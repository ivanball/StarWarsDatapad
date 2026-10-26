# Backlog: warm the cache at startup and serve stale data when SWAPI is down
Labels: backlog, api, estimate: 1h

**Estimate:** 1h

Out of scope for the timebox. Load the six collections in a hosted service at startup and keep the last good
copy when a refresh fails, so the first user does not pay the cold-cache latency and an upstream outage after
the first load is invisible. For more than one instance, move to a distributed cache.
