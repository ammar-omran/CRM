// Pagination

export interface PaginationResponse<T> {
  items: T[];
  pageIndex: number;
  pageSize: number;
  totalCount: number;
}

export interface PaginationRequest {
  skip?: number;
  limit?: number;
  skipTotal?: boolean;
}

//API Wrapper

export interface ApiResponse<T> {
  data: T;
  status: {
    code: number;
    message: string;
  };
}

//Organization

export interface OrganizationResponse {
  id: number;
  name: string;
  createdAt: string;
  agentsCount: number;
  customersCount: number;
}

export interface GetOrganizationsRequest extends PaginationRequest {
  name?: string;
}

//Create Organization

export interface AddOrganizationRequest {
  name: string;
  agents: AddOrganizationAgent[];
  customers: AddOrganizationCustomer[];
}

export interface AddOrganizationAgent {
  id: number;   // Agent ID
  role: number; // Role ID within the organization
}

export interface AddOrganizationCustomer {
  id: number;   // Customer reference ID from CustomerManagement microservice
}

// Organization Customers

export interface OrganizationCustomerResponse {
  name: string;
  email: string;
  phone: string | null; // nullable — may not be provided
  organizationName: string;
}

export interface GetOrganizationCustomersRequest extends PaginationRequest {
  name?: string;
  email?: string;
  organizationName?: string;
}
