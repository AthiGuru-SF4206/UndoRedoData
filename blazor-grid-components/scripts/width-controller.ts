import { isNullOrUndefined } from '@syncfusion/ej2-base';
import { SfGrid } from './sf-grid-fn';
import { Column } from './interfaces';
import { formatUnit } from '@syncfusion/ej2-base';
import { parentsUntil } from './util';

/**
 * ColumnWidthService
 *
 * @hidden
 */
export class ColumnWidthService {
    private parent: SfGrid;

    constructor(parent: SfGrid) {
        this.parent = parent;
    }

    public setMinwidthBycalculation(tWidth?: number): void {
        let difference: number = 0;
        const collection: Column[] = this.parent.getColumns().filter((a: Column) => {
            return isNullOrUndefined(a.width) || a.width === 'auto' || a.width === '';
        });
        if (collection.length) {
            if (!isNullOrUndefined(this.parent.options.width) && this.parent.options.width !== 'auto' && this.parent.options.width.toString().indexOf('%') === -1) {
                difference = (typeof this.parent.options.width === 'string' ? parseInt(this.parent.options.width, 10) : this.parent.options.width) - tWidth;
            }
            else {
                difference = this.parent.element.getBoundingClientRect().width - tWidth;
            }
            let tmWidth: number = 0;
            let minWidth: number = 0;
            for (const cols of collection) {

                tmWidth += !isNullOrUndefined(cols.minWidth) ?
                    ((typeof cols.minWidth === 'string' ? parseInt(cols.minWidth, 10) : cols.minWidth)) : 0;
            }
            const minWidthValues: {} = {};
            for (let i: number = 0; i < collection.length; i++) {
                if (tWidth === 0 && this.parent.options.allowResizing && this.isWidthUndefined() && (i !== collection.length - 1)) {
                    this.setUndefinedColumnWidth(collection);
                }
                if (tWidth !== 0 && difference < tmWidth) {
                    minWidthValues[collection[parseInt(i.toString(), 10)].field] = collection[parseInt(i.toString(), 10)].minWidth + 'px';
                    minWidth += parseInt(collection[parseInt(i.toString(), 10)].minWidth.toString(), 10);
                } else if (tWidth !== 0 && difference > tmWidth) {
                    minWidthValues[collection[parseInt(i.toString(), 10)].field] = '';
                    minWidth += 0;
                }
            }
            this.parent.dotNetRef.invokeMethodAsync('SetMinWidth', minWidthValues);
            if (this.parent.options.frozenColumns) {
                this.parent.freezeModule.setFrozenHeight(minWidth);
            }
        }
    }

    public setUndefinedColumnWidth(collection?: Column[]): void {
        for (let k: number = 0; k < collection.length; k++) {
            if (k !== collection.length - 1) {
                collection[parseInt(k.toString(), 10)].width = 200;
                this.setWidth(200, this.parent.getColumnIndexByField(collection[parseInt(k.toString(), 10)].field));
            }
        }
    }

    public setColumnWidth(column: Column, index?: number, module?: string,
                          allowStopEvent: boolean = true, virtualAutoFit: boolean = false): void {
        if (this.parent.getColumns(virtualAutoFit).length < 1) {
            return;
        }
        let columnIndex: number | undefined;
        if (this.parent.options.enableColumnVirtualization) {
            columnIndex = this.parent.options.virtualizedColumns.findIndex((col: Column) => col.uid === column.uid)
            + this.parent.getIndentCount();
        } else {
            columnIndex = isNullOrUndefined(index) ? this.parent.getNormalizedColumnIndex(column.uid) : index;
        }
        let cWidth: string | number = this.getWidth(column);
        const tgridWidth: number = this.getTableWidth(this.parent.getColumns(virtualAutoFit));
        if (cWidth !== null) {
            this.setWidth(cWidth, columnIndex);
            if (this.parent.options.width !== 'auto' && this.parent.options.width.toString().indexOf('%') === -1) {
                this.setMinwidthBycalculation(tgridWidth);
            }
            if (this.parent.options.enableColumnVirtualization && module === 'resize') {
                this.parent.options.virtualizedColumns.filter((col: Column) => col.uid === column.uid)[0].width = cWidth;
                this.parent.virtualContentModule.refreshOffsets();
                this.parent.virtualContentModule.setVirtualHeight();
            }
            else if (this.parent.options.enableColumnVirtualization && virtualAutoFit) {
                this.parent.options.columns.filter((col: Column) => col.uid === column.uid)[0].width = cWidth;
                this.parent.virtualContentModule.refreshOffsets();
            }
            if ((this.parent.options.allowResizing && module === 'resize') || (this.parent.options.frozenColumns && this.parent.options.allowResizing)) {
                this.setWidthToTable(null, false, 'resize');
            }
            if (allowStopEvent) {
                if (cWidth.toString().indexOf('px') > 0) {
                    cWidth = cWidth.toString().replace('px', '');
                }
                const isResizing: boolean = module === 'resize' ? true : false;
                this.parent.dotNetRef.invokeMethodAsync('ColumnWidthChanged', { index: columnIndex, width: cWidth, columnUid: column.uid }, isResizing);
            }
        }
    }

    public setWidth(width: string | number, index: number, clear?: boolean): void {
        const chrome: string = 'chrome';
        const webstore: string = 'webstore';
        if (typeof (width) === 'string' && width.indexOf('%') !== -1 &&
            !(Boolean(window[`${chrome}`]) && Boolean(window[`${chrome}`][`${webstore}`])) && this.parent.options.allowGrouping) {
            const elementWidth: number = this.parent.element.offsetWidth;
            width = parseInt(width, 10) / 100 * (elementWidth);
        }
        const header: Element = this.parent.getHeaderTable();
        const content: Element = this.parent.getContentTable();
        const fWidth: string = formatUnit(width);
        const headerCol: HTMLTableColElement = (<HTMLTableColElement>header.querySelector('colgroup').children[parseInt(index.toString(), 10)]);
        if (headerCol && !clear) {
            headerCol.style.width = fWidth;
        } else if (headerCol && clear) {
            headerCol.style.width = ' ';
        }
        const contentCol: HTMLTableColElement = (<HTMLTableColElement>content.querySelector('colgroup').children[parseInt(index.toString(), 10)]);
        if (contentCol && !clear) {
            contentCol.style.width = fWidth;
        } else if (contentCol && clear) {
            contentCol.style.width = ' ';
        }

        if (this.parent.options.aggregatesCount !== 0) {
            const tcolGroup: Element = this.parent.getFooterContent().querySelector('colgroup');
            const footerCol: HTMLTableColElement | null = !isNullOrUndefined(tcolGroup) ?
            <HTMLTableColElement>tcolGroup.children[parseInt(index.toString(), 10)] : null;

            if (contentCol && footerCol && !clear) {
                footerCol.style.width = fWidth;
            } else if (contentCol && footerCol && clear) {
                footerCol.style.width = ' ';
            }
        }

        const edit: NodeListOf<Element> = this.parent.element.querySelectorAll('.e-table.e-inline-edit');
        const editTableCol: HTMLTableColElement[] = [];
        for (let i: number = 0; i < edit.length; i++) {
            if (parentsUntil(edit[parseInt(i.toString(), 10)], 'e-grid').id === this.parent.element.id) {
                for (let j: number = 0; j < edit[parseInt(i.toString(), 10)].querySelector('colgroup').children.length; j++) {
                    editTableCol.push((<HTMLTableColElement>edit[parseInt(i.toString(), 10)].querySelector('colgroup').children[parseInt(j.toString(), 10)]));
                }
            }
        }
        if (edit.length && editTableCol.length) {
            editTableCol[parseInt(index.toString(), 10)].style.width = fWidth;
        }
        if (this.parent.options.frozenColumns !== 0 && !this.parent.options.enableColumnVirtualization) {
            this.parent.freezeModule.setFrozenHeight();
        }
    }
    private getColumnLevelFrozenColgroup(index: number, left: number, movable: number, ele: Element): HTMLTableColElement {
        if (!ele || !ele.querySelector('colgroup')) {
            return null;
        }
        const columns: Column[] = this.parent.options.enableColumnVirtualization ?
            this.parent.options.virtualizedColumns : this.parent.frozenColumnModel;
        let headerCol: HTMLTableColElement;
        const colGroup: Element[] = [].slice.call(ele.querySelector('colgroup').children);
        if (columns[parseInt(index.toString(), 10)].freeze === 'Left' && columns[parseInt(index.toString(), 10)].isFrozen) {
            headerCol = colGroup[parseInt(index.toString(), 10)] as HTMLTableColElement;
        }
        else if (columns[parseInt(index.toString(), 10)].freeze === 'Right' && columns[parseInt(index.toString(), 10)].isFrozen) {
            headerCol = colGroup[index - (left + movable)] as HTMLTableColElement;
        }
        else {
            headerCol = colGroup[index - left] as HTMLTableColElement;
        }
        return headerCol;
    }

    public isWidthUndefined(): boolean {
        const isWidUndefCount: number = this.parent.getColumns().filter((col: Column) => {
            return isNullOrUndefined(col.width) && isNullOrUndefined(col.minWidth);
        }).length;
        return (this.parent.getColumns().length === isWidUndefCount);
    }

    public getWidth(column: Column): string | number {

        //TODO: move it to c# side

        // if (isNullOrUndefined(column.width) && this.parent.options.allowResizing
        //     && isNullOrUndefined(column.minWidth) && !this.isWidthUndefined()) {
        //     column.width = 200;
        // }
        // if (this.parent.options.frozenColumns && isNullOrUndefined(column.width) &&
        //     column.index < this.parent.options.frozenColumns) {
        //     column.width = 200;
        // }
        if (!column.width) { return null; }
        const width: number = parseInt(column.width.toString(), 10);
        if (column.minWidth && width < parseInt(column.minWidth.toString(), 10)) {
            return column.minWidth;
        } else if ((column.maxWidth && width > parseInt(column.maxWidth.toString(), 10))) {
            return column.maxWidth;
        } else {
            return column.width;
        }
    }

    public getTableWidth(columns: Column[]): number {
        let tWidth: number = 0;
        for (const column of columns) {
            let cWidth: string | number = this.getWidth(column);
            if (column.width === 'auto') {
                cWidth = 0;
            }
            if (column.visible !== false && cWidth !== null) {
                tWidth += parseInt(cWidth.toString(), 10);
            }
        }
        return tWidth;
    }

    private calcMovableOrFreezeColWidth(tableType: string): string {
        const columns: Column[] = this.parent.frozenColumnModel.length !== 0 ?
            this.parent.frozenColumnModel.slice() : this.parent.getColumns().slice();
        let frozenColumnsCount: number = 0;
        if (!this.parent.options.frozenLeftColumnsCount && !this.parent.options.frozenRightColumnsCount) {
            for (let i: number = 0; i < columns.length; i++) {
                if (columns[parseInt(i.toString(), 10)].index < this.parent.options.actualFrozenColumns
                && !columns[parseInt(i.toString(), 10)].isFrozen) {
                    frozenColumnsCount++;
                }
            }
            this.parent.options.actualFrozenColumns = frozenColumnsCount;
        }
        const left: number = this.parent.options.frozenLeftColumnsCount || this.parent.options.actualFrozenColumns;
        const movable: number = columns.length - this.parent.options.frozenColumns;
        if (tableType === 'movable') {
            if (this.parent.options.frozenRightColumnsCount) {
                columns.splice(left + movable, columns.length);
            }
            if (left) {
                columns.splice(0, left);
            }
        }
        else if (tableType === 'freeze-left') {
            columns.splice(left, columns.length);
        }
        else if (tableType === 'freeze-right') {
            columns.splice(0, left + movable);
        }
        return formatUnit(this.getTableWidth(columns));
    }

    private setWidthToFrozenLeftTable(width?: string): void {
        let freezeWidth: string = isNullOrUndefined(width) ? this.calcMovableOrFreezeColWidth('freeze-left') : width;
        freezeWidth = this.parent.getContent().querySelector('.e-frozen-left-content').classList.contains('e-frozenborderdisabled') ? '0' : freezeWidth;
        (this.parent.getHeaderTable() as HTMLTableElement).style.width = freezeWidth;
        (this.parent.getContentTable() as HTMLTableElement).style.width = freezeWidth;
        this.parent.resizeModule.leftFrozenTableWidth = freezeWidth;
        if (this.parent.getFooterContent() && !isNullOrUndefined(this.parent.getFooterContent().querySelector('.e-frozen-left-footercontent'))) {
            (this.parent.getFooterContent().querySelector('.e-frozen-left-footercontent') as HTMLElement).style.width = freezeWidth;
        }
    }

    private setWidthToFrozenRightTable(width?: string): void {
        let freezeWidth: string = isNullOrUndefined(width) ? this.calcMovableOrFreezeColWidth('freeze-right') : width;
        freezeWidth = this.parent.getContent().querySelector('.e-frozen-right-content').classList.contains('e-frozenborderdisabled') ? '0' : freezeWidth;
        if (!this.parent.options.enableColumnVirtualization) {
            (this.parent.getHeaderContent().querySelector('.e-frozen-right-header').querySelector('.e-table') as HTMLTableElement).style.width = freezeWidth;
            (this.parent.getContent().querySelector('.e-frozen-right-content').querySelector('.e-table') as HTMLTableElement).style.width = freezeWidth;
            this.parent.resizeModule.rightFrozenTableWidth = freezeWidth;
            if (this.parent.getFooterContent() && !isNullOrUndefined(this.parent.getFooterContent().querySelector('.e-frozen-right-footercontent'))) {
                (this.parent.getFooterContent().querySelector('.e-frozen-right-footercontent') as HTMLElement).style.width = freezeWidth;
            }
        }
    }

    private setWidthToMovableTable(width?: string): void {
        let movableWidth: string = '';
        if (isNullOrUndefined(width)) {
            const isColUndefined: boolean = this.parent.getColumns().filter((a: Column) => 
                { return isNullOrUndefined(a.width); }).length >= 1;
            const isWidthAuto: boolean = this.parent.getColumns().filter((a: Column) => { return (a.width === 'auto'); }).length >= 1;
            if (typeof this.parent.options.width === 'number' && !isColUndefined && !isWidthAuto) {
                movableWidth = formatUnit(this.parent.options.width - parseInt(this.calcMovableOrFreezeColWidth('freeze').split('px')[0], 10) - 5);
            } else if (!isColUndefined && !isWidthAuto) {
                movableWidth = this.calcMovableOrFreezeColWidth('movable');
            }
        } else {
            movableWidth = width;
        }
        if (this.parent.getHeaderContent().querySelector('.e-movableheader').firstElementChild && !this.parent.options.enableColumnVirtualization) {
            (this.parent.getHeaderContent().querySelector('.e-movableheader').firstElementChild as HTMLTableElement).style.width
                = movableWidth;
        }

        if (this.parent.getFooterContent() && this.parent.getFooterContent().querySelector('.e-movablefootercontent').firstElementChild && !this.parent.options.enableColumnVirtualization) {
            (<HTMLElement>this.parent.getFooterContent().querySelector('.e-movablefootercontent').firstElementChild).style.width = movableWidth;
        }
        if (!this.parent.options.enableColumnVirtualization) {
            (this.parent.getContent().querySelector('.e-movablecontent').firstElementChild as HTMLTableElement).style.width =
                movableWidth;
            this.parent.resizeModule.tableWidth = movableWidth;
        }
    }
    private setWidthToFrozenEditTable(): void {
        const freezeWidth: string = this.calcMovableOrFreezeColWidth('freeze');
        (this.parent.element.querySelectorAll('.e-table.e-inline-edit')[0] as HTMLTableElement).style.width = freezeWidth;
    }
    private setWidthToMovableEditTable(): void {
        const movableWidth: string = this.calcMovableOrFreezeColWidth('movable');
        if (!isNullOrUndefined(this.parent.element.querySelectorAll('.e-table.e-inline-edit')[1])) {
            (this.parent.element.querySelectorAll('.e-table.e-inline-edit')[1] as HTMLTableElement).style.width = movableWidth;
        }
    }

    public setPersistedWidth(column: Column): void {
        if (this.parent.options.frozenColumns) {
            if (this.parent.options.frozenRightColumnsCount !== 0 || this.parent.options.frozenLeftColumnsCount !== 0) {
                if (this.parent.options.frozenLeftColumnsCount !== 0) {
                    this.setWidthToFrozenLeftTable(column.leftFrozenTableWidth);
                }
                if (this.parent.options.frozenRightColumnsCount !== 0) {
                    this.setWidthToFrozenRightTable(column.rightFrozenTableWidth);
                }
                this.setWidthToMovableTable(column.tableWidth);
            } else {
                this.setWidthToFrozenLeftTable(column.leftFrozenTableWidth);
                this.setWidthToMovableTable(column.tableWidth);
            }
        } else {
            (this.parent.getHeaderTable() as HTMLTableElement).style.width = column.tableWidth;
            (this.parent.getContentTable() as HTMLTableElement).style.width = column.tableWidth;
            if (this.parent.options.aggregatesCount !== 0) {
                (this.parent.getFooterContent().querySelector('.e-table') as HTMLTableElement).style.width = column.tableWidth;
            }
        }
    }
    public setWidthToTable(columns: Column[] = null, tableWidth: boolean = false, module: string = ''): void {
        let tWidth: string;
        if (this.parent.options.enableColumnVirtualization && module === 'resize') {
            tWidth = formatUnit(this.getTableWidth(this.parent.options.virtualizedColumns));
        } else {
            tWidth = formatUnit(this.getTableWidth(columns != null ? columns : <Column[]>this.parent.getColumns()));
        }
        const autoFitColumns: Column[] = this.parent.getColumns().filter((c: Column) => c.autoFit === true && c.visible === true);
        if (!this.parent.options.frozenColumns || (this.parent.options.frozenColumns && !this.parent.options.enableColumnVirtualization && (autoFitColumns.length > 0 || this.parent.options.autoFit)) || (this.parent.options.frozenColumns && module === 'resize')) {
            if (this.parent.options.hasDetailTemplate) {
                this.setWidth('30', 0);
            }
            if (tableWidth) {
                tWidth = '';
            }
            this.parent.resizeModule.tableWidth = tWidth;
            (this.parent.getHeaderTable() as HTMLTableElement).style.width = tWidth;
            (this.parent.getContentTable() as HTMLTableElement).style.width = tWidth;
            if (this.parent.options.aggregatesCount !== 0 && !isNullOrUndefined((this.parent.getFooterContent().querySelector('.e-table') as HTMLTableElement))) {
                (this.parent.getFooterContent().querySelector('.e-table') as HTMLTableElement).style.width = tWidth;
            }
        }
        const edit: HTMLTableElement = <HTMLTableElement>this.parent.element.querySelector('.e-table.e-inline-edit');
        if (edit && this.parent.options.frozenColumns) {
            this.setWidthToFrozenEditTable();
            this.setWidthToMovableEditTable();
        } else if (edit) {
            edit.style.width = tWidth;
        }
    }
}
