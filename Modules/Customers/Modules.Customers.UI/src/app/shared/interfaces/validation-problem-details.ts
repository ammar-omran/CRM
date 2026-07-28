export interface ValidationProblemDetails {
  errors: {
    [field: string]: string[];
  };
  title?: string;
  status?: number;
  detail?: string;
}
