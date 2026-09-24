import { BsDropdownModule } from 'ngx-bootstrap/dropdown';

// Patch for ngx-intl-tel-input 17.x which expects BsDropdownModule.forRoot()
// ngx-bootstrap 22+ removed forRoot() (now standalone). This must run before
// ngx-intl-tel-input is evaluated, so it is imported as the very first line in main.ts.
if (!(BsDropdownModule as unknown as { forRoot?: unknown }).forRoot) {
  (BsDropdownModule as unknown as { forRoot: () => unknown }).forRoot = (() =>
    ({ ngModule: BsDropdownModule, providers: [] }) as unknown as never) as unknown as () => unknown;
}
