export const EMAIL_CONFIG = {
  // Support contact information
  SUPPORT_EMAIL: 'support@azka.com',
  SUPPORT_PHONE: '+1234567890',
  
  // Company information
  COMPANY_NAME: 'Azka',
  COMPANY_WEBSITE: 'https://azka.com',
  
  // Email templates
  TEMPLATES: {
    TICKET_CREATION: 'ticket-creation',
    TICKET_UPDATE: 'ticket-update',
    TICKET_RESOLVED: 'ticket-resolved'
  },
  
  // Email subjects
  SUBJECTS: {
    TICKET_CREATED: 'Ticket Created Successfully - {ticketReference}',
    TICKET_UPDATED: 'Ticket Updated - {ticketReference}',
    TICKET_RESOLVED: 'Ticket Resolved - {ticketReference}'
  },
  
  // Email settings
  SETTINGS: {
    FROM_EMAIL: 'noreply@azka.com',
    FROM_NAME: 'Azka Support Team',
    REPLY_TO: 'support@azka.com'
  }
};
