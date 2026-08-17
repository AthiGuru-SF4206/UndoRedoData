import { SfGrid } from './sf-grid-fn';
import { calculateRelativeBasedPosition } from '@syncfusion/ej2-popups';
import { isNullOrUndefined } from '@syncfusion/ej2-base';
import { Column, OffsetPosition } from './interfaces';

/**
 * The `ColumnChooser` module is used to show or hide columns dynamically.
 */
export class ColumnChooser  {
    private parent: SfGrid;
    private mediaCol: Column[] = [];
    private media: { [key: string]: MediaQueryList } = {};
    private mediaBindInstance: Object = {};
    private mediaColVisibility: { [key: string]: boolean } = {};

    constructor(parent: SfGrid) {
        this.parent = parent;
    }

    /**
     * Get columnChooser Position.
     *
     * @returns {void}
     * @hidden
     */
    public renderColumnChooser(): any {
        const dlgelement: HTMLElement = this.parent.element.querySelector('#' + this.parent.element.id + '_ccdlg');
        const stickyHeader : boolean = this.parent.getHeaderContent().parentElement.classList.contains('e-sticky');
        let newpos: OffsetPosition;
        if (stickyHeader) {
            dlgelement.classList.add('e-sticky');
        }
    }

    public setMediaColumns(isResetPersistData?: boolean): void {
        const columns: Column[] = this.parent.getColumns();
        const isPersistEnabled: boolean = this.parent.options.enablePersistence;
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const persistData: Record<string, any> | null = isPersistEnabled
            ? JSON.parse(window.localStorage.getItem('grid' + this.parent.element.id) as string)
            : null;
        if (!isNullOrUndefined(columns)) {
            for (let index: number = 0; index < columns.length; index++) {
                const column: Column = columns[index as number];
                const isVisible: boolean = isNullOrUndefined(column.visible) || column.visible || isResetPersistData;
                if (column.hideAtMedia !== '' && isVisible) {
                    this.pushMediaColumn(column, index);
                    if (isPersistEnabled && !isNullOrUndefined(persistData) && !isNullOrUndefined(this.mediaColVisibility[column.uid])) {
                        persistData.columns[index as number].visible = this.mediaColVisibility[column.uid];
                    }
                }
            }
            if (isPersistEnabled && !isNullOrUndefined(persistData)) {
                window.localStorage.setItem('grid' + this.parent.element.id, JSON.stringify(persistData));
            }
            this.parent.dotNetRef.invokeMethodAsync('SetMediaColumnVisibility', {
                mediaColVisibility: this.mediaColVisibility
            });
            this.mediaColVisibility = {};
        }
    }

    public windowResized(): void {
        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const _this: ColumnChooser = this;
        setTimeout(function(): void {
            if (!isNullOrUndefined(_this.mediaColVisibility) && Object.keys(_this.mediaColVisibility).length > 0) {
                _this.parent.dotNetRef.invokeMethodAsync('SetMediaColumnVisibility', {
                    mediaColVisibility: _this.mediaColVisibility,
                    invokedByMedia: true
                });
                _this.mediaColVisibility = {};
            }
        }, 100);
    }

    private pushMediaColumn(col: Column, index: number): void {
        this.mediaCol.push(col);
        this.media[col.uid] = window.matchMedia(col.hideAtMedia);
        this.mediaQueryUpdate(index, this.media[col.uid]);
        this.mediaBindInstance[parseInt(index.toString(), 10)] = this.mediaQueryUpdate.bind(this, index);
        this.media[col.uid].addListener(this.mediaBindInstance[parseInt(index.toString(), 10)] as null);
    }

    private mediaQueryUpdate(columnIndex: number, e: MediaQueryList): void {
        const col: Column = this.parent.getColumns()[parseInt(columnIndex.toString(), 10)];
        if (this.mediaCol.some((mediaColumn: Column) => mediaColumn.uid === col.uid)) {
            this.mediaColVisibility[col.uid] = e.matches;
        }
    }

    public updateMediaColumns(mediaColumnsUid: { [uid: string]: boolean }): void {
        const keys: string[] = Object.keys(mediaColumnsUid);
        for (let i: number = 0; i < keys.length; i++) {
            let idxToSplice: number = -1;
            if (this.mediaCol.some((mCol: Column) => {
                idxToSplice++;
                return mCol.uid === keys[parseInt(i.toString(), 10)];
            })) {
                this.mediaCol.splice(idxToSplice, 1);
            } else {
                this.pushMediaColumn(this.parent.getColumnByUid(keys[parseInt(i.toString(), 10)]),
                                     this.parent.getColumnIndexByUid(keys[parseInt(i.toString(), 10)]));
            }
        }
    }

    public removeMediaListener(): void {
        for (let i: number = 0; i < this.mediaCol.length; i++) {
            this.media[this.mediaCol[parseInt(i.toString(), 10)].uid].
                removeListener(this.mediaBindInstance[this.mediaCol[parseInt(i.toString(), 10)].index]as null);
        }
    }
}
