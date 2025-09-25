#if USE_DATA_CACHING
const cacheName = {{{JSON.stringify(COMPANY_NAME + "-" + PRODUCT_NAME + "-" + PRODUCT_VERSION )}}};
const contentToCache = [
  "Build/{{{ LOADER_FILENAME }}}",
  "Build/{{{ FRAMEWORK_FILENAME }}}",
  #if USE_THREADS
  "Build/{{{ WORKER_FILENAME }}}",
  #endif
  "Build/{{{ DATA_FILENAME }}}",
  "Build/{{{ CODE_FILENAME }}}",
  "TemplateData/style.css"
];
#endif

self.addEventListener('install', (e) => {
  console.log('[Service Worker] Install');
  #if USE_DATA_CACHING
  e.waitUntil((async () => {
    const cache = await caches.open(cacheName);
    await cache.addAll(contentToCache.map(p => new Request(p, { cache: 'reload' })));
  })());
  self.skipWaiting();
  #endif
});

#if USE_DATA_CACHING
self.addEventListener('fetch', (e) => {
  const req = e.request;
  if (req.method !== 'GET') return;

  const url = new URL(req.url);
  const sameOrigin = url.origin === self.location.origin;

  const isStatic =
    sameOrigin && /\.(js|css|wasm|data|png|jpg|jpeg|gif|webp|svg|ico)$/i.test(url.pathname);

  if (!isStatic) {
    e.respondWith((async () => {
      try {
        return await fetch(req);
      } catch (err) {
        return new Response('', { status: 502, statusText: 'Bad Gateway' });
      }
    })());
    return;
  }

  e.respondWith((async () => {
    const hit = await caches.match(req);
    if (hit) return hit;
    try {
      const resp = await fetch(req);
      if (resp.ok && resp.type !== 'opaque') {
        const cache = await caches.open(cacheName);
        await cache.put(req, resp.clone());
      }
      return resp;
    } catch (err) {
      return new Response('', { status: 504, statusText: 'Gateway Timeout' });
    }
  })());
});
#endif