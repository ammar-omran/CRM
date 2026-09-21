import { Direction, Directionality } from '@angular/cdk/bidi';
import { EventEmitter, Injectable, OnDestroy, signal, WritableSignal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AppDirectionality implements Directionality, OnDestroy {
  readonly change = new EventEmitter<Direction>();
  readonly valueSignal: WritableSignal<Direction> = signal<Direction>('ltr');

  get value(): Direction {
    return this.valueSignal();
  }
  set value(value: Direction) {
    this.valueSignal.set(value);
    this.change.next(value);
  }

  ngOnDestroy() {
    this.change.complete();
  }
}
