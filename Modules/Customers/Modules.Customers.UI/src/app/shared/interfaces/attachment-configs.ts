import { AttachmentType } from "@shared/Enums/attachment-type";

export interface AttachmentConfigs {
  limitNumber: number;
  maxSizeMB: number;
  allowedExtensions: string[];
}
