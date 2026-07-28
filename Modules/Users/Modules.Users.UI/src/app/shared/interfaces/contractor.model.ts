export interface Contractor {
  id?: number;
  name?: string;
  contactPersonName?: string;
  officeAddress?: string;
  code?: string;
  assignedDistricts: number[];
  phone: string;
  ghanaPostAddress: string;
}
