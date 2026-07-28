import { Directive, Input, OnDestroy, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';
import { Subscription } from 'rxjs';
import { RbacService } from '@core/authentication';

/**
 * Structural directive that conditionally renders its host element
 * based on whether the current user holds the specified permission.
 *
 * @example
 * ```html
 * <!-- Show only if user has TicketComments.Add permission -->
 * <button *hasPermission="'TicketComments.Add'">Add Comment</button>
 *
 * <!-- Show for multiple permissions -->
 * <div *hasPermission="['TicketComments.Add', 'TicketAttachments.Upload']">...</div>
 * ```
 */
@Directive({
  selector: '[hasPermission]',
  standalone: true,
})
export class HasPermissionDirective implements OnInit, OnDestroy {
  private _required: string | string[] = [];
  private _subscription?: Subscription;
  private _hasView = false;

  @Input()
  set hasPermission(permission: string | string[]) {
    this._required = permission;
  }

  constructor(
    private templateRef: TemplateRef<unknown>,
    private viewContainer: ViewContainerRef,
    private rbacService: RbacService
  ) {}

  ngOnInit(): void {
    this._subscription = this.rbacService.permissions().subscribe(() => {
      this._updateView();
    });
  }

  ngOnDestroy(): void {
    this._subscription?.unsubscribe();
  }

  private _updateView(): void {
    const required = Array.isArray(this._required) ? this._required : [this._required];
    const allowed = required.some(p => this.rbacService.hasPermission(p));

    if (allowed && !this._hasView) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this._hasView = true;
    } else if (!allowed && this._hasView) {
      this.viewContainer.clear();
      this._hasView = false;
    }
  }
}
