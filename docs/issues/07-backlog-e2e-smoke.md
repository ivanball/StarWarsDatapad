# Backlog: end-to-end smoke test in a real browser
Labels: backlog, testing, estimate: 1h

**Estimate:** 1h

Out of scope for the timebox. A Playwright test that starts the app against a stubbed upstream, selects a
character and asserts the details render. It catches wiring problems (static file hosting, routing,
serialization between the two processes) that component and API tests cannot see.
