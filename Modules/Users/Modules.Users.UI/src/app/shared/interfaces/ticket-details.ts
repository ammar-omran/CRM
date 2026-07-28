export interface TicketDetails {
  id: number;
  title: string;
  description: string;
  createdAt: string;
  lastUpdate: string;
  status: string;
  category: string;
  service: string;
  severity: string;
  type: string;
  customerName: string;
  customerEmail: string; // Added customer email for notifications
  assignedTo: string;
  updatedByName?: string;
  // Alternative field names that might be used
  UpdatedByName?: string;
  assignedToName?: string;
  AssignedToName?: string;
}
