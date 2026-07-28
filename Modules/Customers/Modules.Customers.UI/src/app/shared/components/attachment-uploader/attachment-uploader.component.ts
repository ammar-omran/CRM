import { Component, EventEmitter, inject, Input, OnInit, Output } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ApiService } from '@shared/services/api.service';
import { HelperService } from '@shared/services/helper.service';
import { EndPoint, HttpVerb } from '@shared/enums';
import { AttachmentType } from '@shared/Enums/attachment-type';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { AttachmentConfigs } from '@shared/interfaces/attachment-configs';
import { ToastrService } from 'ngx-toastr';
import { environment as env } from '@env/environment';

@Component({
  selector: 'app-attachment-uploader',
  standalone: true,
  templateUrl: './attachment-uploader.component.html',
  styleUrls: ['./attachment-uploader.component.scss'],
  imports: [CommonModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule],
})
export class AttachmentUploaderComponent implements OnInit {
  @Input() refId: number | null = null;
  @Input() attachmentFor!: AttachmentType;
  @Output() attachmentsChange = new EventEmitter<any[]>();
  configs!: AttachmentConfigs;

  private readonly configKeyMap: Record<
    AttachmentType,
    keyof typeof env.AttachmentsConfigurations
  > = {
    [AttachmentType.ticket]: 'ticket',
    [AttachmentType.comment]: 'comment',
    [AttachmentType.none]: 'ticket',
  };

  ngOnInit(): void {
    this.configs = env.AttachmentsConfigurations[
      this.configKeyMap[this.attachmentFor]
    ] as AttachmentConfigs;
  }

  private toastr = inject(ToastrService);

  uploadedAttachments: { id: number; name: string }[] = [];
  isSubmittingAttachment = false;
  expanded = false;

  constructor(private apiService: ApiService) {}

  async chooseFiles(): Promise<File[]> {
    return new Promise(resolve => {
      const input = document.createElement('input');
      input.type = 'file';
      input.multiple = true;

      input.onchange = () => {
        const files = input.files ? Array.from(input.files) : [];

        if (files.length === 0) {
          resolve([]);
        } else {
          resolve(files);
        }

        input.remove();
      };

      input.click();
    });
  }

  async uploadAttachment(): Promise<void> {
    this.isSubmittingAttachment = true;
    const selectedFiles = await this.chooseFiles();

    if (!selectedFiles.length) {
      this.isSubmittingAttachment = false;
      return;
    }

    for (const file of selectedFiles) {
      const validation = this.validateFile(file);
      if (!validation.valid) {
        this.toastr.error(validation.message ?? "Can't upload file", 'Attachment Validation Error');
        this.isSubmittingAttachment = false;
        continue;
      }

      const formData = new FormData();
      formData.append('file', file);

      const uploadAttachmentEndpoint = HelperService.formatEndpoint(EndPoint.UPLOAD_ATTACHMENT, {
        attachmentTypeId: this.attachmentFor,
        referenceId: this.refId ?? 0,
      }) as EndPoint;

      this.apiService
        .triggerApiRequest<any>(uploadAttachmentEndpoint, HttpVerb.POST, undefined, formData)
        .subscribe({
          next: (response: any) => {
            this.isSubmittingAttachment = false;
            const fileName = response?.data?.fileName || file.name;
            const fileId = response?.data?.fileId;
            this.uploadedAttachments.push({ id: fileId, name: fileName });
            this.attachmentsChange.emit(this.uploadedAttachments);
          },
          error: (error: any) => {
            this.isSubmittingAttachment = false;
            this.toastr.error(error.error?.status?.message ?? '', 'Field to send Attachment');
          },
        });
    }
  }

  clearAttachments(): void {
    this.uploadedAttachments = [];
    this.attachmentsChange.emit(this.uploadedAttachments);
  }

  removeAttachment(attachmentId: number) {
    if (!attachmentId) return;

    const deleteEndpoint = HelperService.formatEndpoint(EndPoint.DELETE_ATTACHMENT, {
      attachmentId,
    }) as EndPoint;

    this.apiService.triggerApiRequest(deleteEndpoint, HttpVerb.DELETE).subscribe({
      next: () => {
        this.uploadedAttachments = this.uploadedAttachments.filter(a => a.id !== attachmentId);
        this.attachmentsChange.emit(this.uploadedAttachments);
        this.toastr.success('Attachment removed successfully.');
      },
      error: (error: HttpErrorResponse) => {
        this.toastr.error('Failed to delete attachment from storage.', 'Error');
      },
    });
  }

  private validateFile(file: File): { valid: boolean; message?: string } {
    if (!file) return { valid: false, message: 'File is empty.' };
    const maxSize = this.configs.maxSizeMB * 1024 * 1024;
    if (file.size > maxSize) {
      return {
        valid: false,
        message: `This file is too large. Max limit is ${this.configs.maxSizeMB} mega byte.`,
      };
    }

    const extension = '.' + file.name.split('.').pop()?.toLowerCase();
    if (!this.configs.allowedExtensions.includes(extension)) {
      return {
        valid: false,
        message: `Unsupported file type. Please upload a file in ${this.configs.allowedExtensions.join(', ')}`,
      };
    }

    if (this.uploadedAttachments.length >= this.configs.limitNumber) {
      return { valid: false, message: `You can only upload ${this.configs.limitNumber} files.` };
    }

    return { valid: true };
  }
}
