import { HttpResponse } from '@angular/common/http';

export function getMimeType(fileType: string): string {
  switch (fileType.toLowerCase()) {
    case 'pdf':
      return 'application/pdf';
    case 'excel':
    case 'xlsx':
      return 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
    case 'csv':
      return 'text/csv';
    default:
      return 'application/octet-stream';
  }
}

export function getFileExtension(fileType: string): string {
  switch (fileType) {
    case 'pdf':
      return 'pdf';
    case 'excel':
      return 'xlsx';
    case 'csv':
      return 'csv';
    default:
      return '';
  }
}

export const FileFormats = [
  { label: 'Excel', value: 'excel' },
  { label: 'CSV', value: 'csv' },
  { label: 'PDF', value: 'pdf' },
];

export function downloadFile(
  response: HttpResponse<Blob>,
  defaultFilename: string,
  format: string
): void {
  const contentDisposition = response.headers.get('content-disposition');
  let filename = defaultFilename;

  if (contentDisposition) {
    const match = contentDisposition.match(/filename="?(.+)"?/);
    if (match && match[1]) filename = match[1];
  }

  const blob = new Blob([response.body!], {
    type: response.headers.get('content-type') || getMimeType(format),
  });

  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();

  setTimeout(() => {
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }, 100);
}
