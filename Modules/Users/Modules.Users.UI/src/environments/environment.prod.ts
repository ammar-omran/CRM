export const environment = {
  production: false,
  ApiUrl: 'http://45.240.58.52:9032/api', // Azka Getaway
  baseUrl: '',
  useHash: false,
  releaseDate: '2025',
  versionNumber: '2.5.0',
  maxImageSize: 5 * 1024 * 1024,
  Key: 'CgkiYpej4VYacsKnFom7ZQ==',
  IV: 'kTo2ADtsGEYKtrzxFjFk5A==',
  AttachmentsConfigurations: {
    ticket: {
      limitNumber: 10,
      maxSizeMB: 2,
      allowedExtensions: ['.jpg', '.jpeg', '.png', '.pdf'],
    },
    comment: {
      limitNumber: 1,
      maxSizeMB: 2,
      allowedExtensions: ['.jpg', '.jpeg', '.png', '.pdf'],
    },
  },
};
