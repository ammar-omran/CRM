//model for admin tickets list
export interface AdminTicket {
  id: number;
  createdAt: string;
  status: string;
  severity: string;
  assignedTo: string;
}
