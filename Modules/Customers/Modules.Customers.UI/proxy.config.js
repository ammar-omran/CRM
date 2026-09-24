// https://angular.io/guide/build#proxying-to-a-backend-server

const PROXY_CONFIG = {
  '/api/**': {
    target: 'https://localhost:55760',
    changeOrigin: true,
    secure: false, // Set to false to ignore SSL certificate issues in development
    logLevel: 'debug',
    // Do not strip '/api'; backend expects '/api' prefix
    // pathRewrite: { '^/api': '' },
    headers: {
      'Access-Control-Allow-Origin': '*',
      'Access-Control-Allow-Methods': 'GET, POST, PUT, DELETE, OPTIONS',
      'Access-Control-Allow-Headers': 'Content-Type, Authorization',
    },
  },
  '/users/**': {
    target: 'https://api.github.com',
    changeOrigin: true,
    secure: false,
    logLevel: 'debug',
  },
};

module.exports = PROXY_CONFIG;

