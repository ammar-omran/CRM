const { createProxyMiddleware } = require('http-proxy-middleware');

module.exports = function (app) {
  app.use(
    '/api',
    createProxyMiddleware({
      target: 'https://localhost:51809',
      changeOrigin: true,
      secure: false,
      logLevel: 'debug',
      onProxyReq: function (proxyReq, req, res) {},
      onProxyRes: function (proxyRes, req, res) {},
      onError: function (err, req, res) {},
    })
  );
};
