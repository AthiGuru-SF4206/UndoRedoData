import { SfGrid } from './sf-grid-fn';
import { Column } from './interfaces';
import { EventHandler, createElement, detach, formatUnit, Browser, closest, isNullOrUndefined } from '@syncfusion/ej2-base';
import { OffsetPosition } from './interfaces';
import { ColumnWidthService } from './width-controller';
import { getScrollBarWidth, parentsUntil, applyStickyLeftRightPosition } from './util';

export const resizeClassList: ResizeClasses = {
    root: 'e-rhandler',
    suppress: 'e-rsuppress',
    icon: 'e-ricon',
    helper: 'e-rhelper',
    header: 'th.e-headercell',
    cursor: 'e-rcursor'
};

export interface ResizeClasses {
    root: string;
    suppress: string;
    icon: string;
    helper: string;
    header: string;
    cursor: string;
}

/**
 * Resize handler
 */

export class Resize {

    private content: HTMLDivElement;
    private header: HTMLDivElement;
    private pageX: number;
    private column: Column;
    private element: HTMLElement;
    private helper: HTMLElement;
    private tapped: boolean | number = false;
    private isDblClk: boolean | number = true;
    private minMove: number;
    private parentElementWidth: number;
    public isFrozenColResized: boolean;
    public tableWidth: string;
    public leftFrozenTableWidth: string;
    public rightFrozenTableWidth: string;
    //Module declarations
    private parent: SfGrid;
    private widthService: ColumnWidthService;

    constructor(parent: SfGrid) {
        this.parent = parent;
        this.widthService = new ColumnWidthService(this.parent);
    }

    /**
     * Resize by field names.
     *
     * @param  {string|string[]} fName - Defines the field name.
     * @returns {void}
     */
    public autoFitColumns(fName?: string | string[]): void {
        const virtualAutoFit: boolean = this.parent.options.enableColumnVirtualization ? true : false;
        let columnName: string[] = [];
        if (fName === undefined || fName === null || fName.length <= 0) {
            if (this.parent.options.frozenColumns > 0) {
                columnName = this.parent.autofitFrozenColumns(true);
            } else {
                columnName = this.parent.getColumns(virtualAutoFit).map((x: Column) => x.field || x.uid);
            }
        } else {
            columnName = (typeof fName === 'string') ? [fName] : fName;
        }
        this.findColumn(columnName);
        if (this.parent.options.frozenColumns > 0) {
            this.widthService.setWidthToTable();
        }
    }

    public autoFit(): void {
        let newarray: string[];
        if (this.parent.options.frozenColumns || this.parent.options.actualFrozenColumns > 0) {
            newarray = this.parent.autofitFrozenColumns();
        }
        else {
            newarray = this.parent.getColumns().filter((c: Column) => c.autoFit === true)
                .map((c: Column) => c.field || c.uid);
        }
        if (newarray.length > 0) {
            this.autoFitColumns(newarray);
        }
    }

    /* tslint:disable-next-line:max-func-body-length */
    private resizeColumn(fName: string, index: number, id?: string, allowStopEvent: boolean = false): void {
        const gObj: SfGrid = this.parent;
        let tWidth: number = 0;
        let footerTable: Element;
        const headerDivTag: string = 'e-gridheader';
        const contentDivTag: string = 'e-gridcontent';
        const footerDivTag: string = 'e-gridfooter';
        let indentWidth: number = 0;
        const autoFitVirtual: boolean = this.parent.options.enableColumnVirtualization ? true : false;
        let uid: string = id ? id : this.parent.getUidByColumnField(fName, autoFitVirtual);
        uid = uid ? uid : fName;
        const columnIndex: number = this.parent.getNormalizedColumnIndex(uid, autoFitVirtual);
        //let headerTextClone: Element;
        let contentTextClone: NodeListOf<Element>;
        let footerTextClone: NodeListOf<Element>;
        let columnIndexByField: number = this.parent.getColumnIndexByField(fName, autoFitVirtual);
        columnIndexByField = columnIndexByField === -1 ? this.parent.getColumnIndexByUid(fName, autoFitVirtual) : columnIndexByField;
        const frzCols: number = gObj.options.frozenColumns;
        if (!isNullOrUndefined(gObj.getFooterContent())) {
            footerTable = gObj.getFooterContent().querySelector('.e-table');
        }
        const headerTable: Element = gObj.getHeaderTable();
        const contentTable: Element = gObj.getContentTable();
        const headerTextClone: Element = (closest(headerTable.querySelector('[e-mappinguid="' + uid + '"]'), 'th')).cloneNode(true) as HTMLElement;
        if (frzCols) {
            const ariaColIndex: string = headerTextClone.getAttribute('aria-colindex');
            contentTextClone = contentTable.querySelectorAll('[aria-colindex="' + ariaColIndex + '"]');
            if (footerTable) {
                footerTextClone = footerTable.querySelectorAll(`td:nth-child(${columnIndex + 1})`);
            }
        }
        else {
            contentTextClone = contentTable.querySelectorAll(`td:nth-child(${columnIndex + 1}):not(.e-groupcaption)`);
            if (footerTable) {
                footerTextClone = footerTable.querySelectorAll(`td:nth-child(${columnIndex + 1}):not(.e-groupcaption)`);
            }
        }
        const indentWidthClone: NodeListOf<Element> = headerTable.querySelector('tr').querySelectorAll('.e-grouptopleftcell');
        if (indentWidthClone.length > 0) {
            for (let i: number = 0; i < indentWidthClone.length; i++) {
                indentWidth += (<HTMLElement>indentWidthClone[parseInt(i.toString(), 10)]).offsetWidth;
            }
        }
        const detailsElement: HTMLElement = <HTMLElement>contentTable.querySelector('.e-detailrowcollapse') ||
            <HTMLElement>contentTable.querySelector('.e-detailrowexpand');
        if ((this.parent.options.hasDetailTemplate) && detailsElement) {
            indentWidth += detailsElement.offsetWidth;
        }
        const headerText: Element[] = [headerTextClone];
        const contentText: Element[] = [];
        const footerText: Element[] = [];
        if (footerTable) {
            for (let i: number = 0; i < footerTextClone.length; i++) {
                footerText[parseInt(i.toString(), 10)] = footerTextClone[parseInt(i.toString(), 10)].cloneNode(true) as Element;
            }
        }
        for (let i: number = 0; i < contentTextClone.length; i++) {
            contentText[parseInt(i.toString(), 10)] = contentTextClone[parseInt(i.toString(), 10)].cloneNode(true) as Element;
        }
        const wHeader: number = this.createTable(headerTable, headerText, headerDivTag);
        const wContent: number = this.createTable(contentTable, contentText, contentDivTag);
        let wFooter: number = null;
        if (footerText.length) {
            wFooter = this.createTable(footerTable, footerText, footerDivTag);
        }
        const columnbyindex: Column = gObj.getColumns(autoFitVirtual)[parseInt(columnIndexByField.toString(), 10)];
        let width: string = columnbyindex.width = formatUnit(Math.max(wHeader, wContent, wFooter));
        if (!isNullOrUndefined(columnbyindex.maxWidth) && columnbyindex.maxWidth !== '' &&
            (parseInt(width, 10) > parseInt(columnbyindex.maxWidth.toString(), 10))) {
            columnbyindex.width = columnbyindex.maxWidth.toString();
        }
        this.widthService.setColumnWidth(gObj.getColumns(autoFitVirtual)[parseInt(columnIndexByField.toString(), 10)] as Column);
        const result: boolean = gObj.getColumns(autoFitVirtual)
            .some((x: Column) => (x.width === null || x.width === undefined || (x.width as string).length <= 0) && x.visible);
        if (result === false) {
            const element: Column[] = (gObj.getColumns(autoFitVirtual) as Column[]);
            for (let i: number = 0; i < element.length; i++) {
                if (element[parseInt(i.toString(), 10)].visible) {
                    tWidth = tWidth + parseFloat(element[parseInt(i.toString(), 10)].width as string);
                }
            }
        }
        const calcTableWidth: number = tWidth + indentWidth;
        if (tWidth > 0 && !gObj.options.frozenColumns) {
            //TODO: why this?
            if (this.parent.options.hasDetailTemplate) {
                //this.widthService.setColumnWidth(new Column({ width: '30px' }));
                this.widthService.setWidth('30', 0);
            }
            (<HTMLTableElement>headerTable).style.width = formatUnit(calcTableWidth);
            (<HTMLTableElement>contentTable).style.width = formatUnit(calcTableWidth);
            if (!isNullOrUndefined(footerTable)) {
                (<HTMLTableElement>footerTable).style.width = formatUnit(calcTableWidth);
            }
        }
        gObj.addTableBorderClass();
        this.parent.freezeModule.refreshRowHeight();
        if (width.toString().indexOf('px') > 0) {
            width = width.replace('px', '');
        }
        this.parent.options.isResizedGrid = true;
        this.parent.dotNetRef.invokeMethodAsync('ColumnWidthChanged', { width: width, columnUid: uid, allowStopEvent: allowStopEvent }, false);
    }

    /**
     * To destroy the resize
     *
     * @returns {void}
     * @hidden
     */
    public destroy(): void {
        const gridElement: Element = this.parent.element;
        if (!gridElement || (!gridElement.querySelector('.e-gridheader') && !gridElement.querySelector('.e-gridcontent'))) { return; }
        this.widthService = null;
        this.unwireEvents();
        //this.removeEventListener();
    }
    /**
     * For internal use only - Get the module name.
     *
     * @private
     * @returns {string} The name of the module.
     */
    protected getModuleName(): string {
        return 'resize';
    }
    private findColumn(fName: string[]): void {
        for (let i: number = 0; i < fName.length; i++) {
            const fieldName: string = fName[parseInt(i.toString(), 10)] as string;
            const autoFitVirtual: boolean = this.parent.options.enableColumnVirtualization ? true : false;
            let columnIndex: number = this.parent.getColumnIndexByField(fieldName, autoFitVirtual);
            columnIndex = columnIndex === -1 ? this.parent.getColumnIndexByUid(fieldName, autoFitVirtual) : columnIndex;
            const column: Column = this.parent.getColumns(autoFitVirtual)[parseInt(columnIndex.toString(), 10)];
            if (columnIndex > -1 && !isNullOrUndefined(column) && column.visible === true) {
                if (!(this.parent.options.allowGrouping && ((!isNullOrUndefined(this.parent.options.initGroupingField) &&
                    this.parent.options.initGroupingField.some((x: string) => x === column.field)) &&
                    !this.parent.options.showGroupedColumn))) {
                    this.resizeColumn(fieldName, columnIndex);
                }
            }
        }
    }
    /**
     * To create table for autofit
     *
     * @hidden
     * @param {Element} table - The table element to create.
     * @param {Element[]} text - Array of text elements.
     * @param {string} tag - The tag string.
     * @returns {number} The created table's number.
     */
    protected createTable(table: Element, text: Element[], tag: string): number {
        const myTableDiv: HTMLDivElement = createElement('div') as HTMLDivElement;
        myTableDiv.className = this.parent.element.className;
        myTableDiv.style.cssText = 'display: inline-block;visibility:hidden;position:absolute';
        const mySubDiv: HTMLDivElement = createElement('div') as HTMLDivElement;
        mySubDiv.className = tag;
        const myTable: HTMLTableElement = createElement('table') as HTMLTableElement;
        myTable.className = table.className;
        myTable.classList.add('e-resizetable');
        myTable.style.cssText = 'table-layout: auto;width: auto';
        let thead: HTMLElement | null = null;
        if (tag === 'e-gridheader') {
            thead = createElement('thead') as HTMLElement;
            myTable.appendChild(thead);
        }
        const myTr: HTMLTableRowElement = createElement('tr') as HTMLTableRowElement;
        for (let i: number = 0; i < text.length; i++) {
            const tr: HTMLTableRowElement = myTr.cloneNode() as HTMLTableRowElement;
            tr.className = table.querySelector('tr').className;
            tr.appendChild(text[parseInt(i.toString(), 10)]);
            if (thead) {
                thead.appendChild(tr);
            } else {
                myTable.appendChild(tr);
            }
        }
        mySubDiv.appendChild(myTable);
        myTableDiv.appendChild(mySubDiv);
        document.body.appendChild(myTableDiv);
        const offsetWidthValue: number = myTable.getBoundingClientRect().width;
        document.body.removeChild(myTableDiv);
        return Math.ceil(offsetWidthValue);
    }
    /**
     * @hidden
     */
    // public addEventListener(): void {
    //     if (this.parent.isDestroyed) {
    //         return;
    //     }
    //     this.parent.on(events.headerRefreshed, this.refreshHeight, this);
    //     this.parent.on(events.initialEnd, this.wireEvents, this);
    //     this.parent.on(events.contentReady, this.autoFit, this);
    // }
    /**
     * @hidden
     */
    // public removeEventListener(): void {
    //     if (this.parent.isDestroyed) {
    //         return;
    //     }
    //     this.parent.off(events.headerRefreshed, this.refreshHeight);
    //     this.parent.off(events.initialEnd, this.wireEvents);
    // }
    /**
     * @hidden
     * @returns {void}
     */
    public render(): void {
        this.unwireEvents();
        this.wireEvents();
        this.setHandlerHeight();
    }

    private refreshHeight(): void {
        const element: HTMLElement[] = this.getResizeHandlers();
        for (let i: number = 0; i < element.length; i++) {
            const headerCellElement: HTMLElement = element[parseInt(i.toString(), 10)] as HTMLElement;
            const headerCellContainer: HTMLElement = headerCellElement.parentElement as HTMLElement;
            if (!isNullOrUndefined(headerCellContainer) && !isNullOrUndefined(headerCellContainer.parentElement)
                && (headerCellContainer.parentElement as HTMLElement).offsetHeight > 0) {
                element[parseInt(i.toString(), 10)].style.height = (headerCellContainer.parentElement as HTMLElement).offsetHeight + 'px';
            }
        }
        this.setHandlerHeight();
    }

    private wireEvents(): void {
        EventHandler.add(this.parent.getHeaderContent(), Browser.touchStartEvent, this.resizeStart, this);
        EventHandler.add(this.parent.getHeaderContent(), 'dblclick', this.callAutoFit, this);
    }

    private unwireEvents(): void {
        EventHandler.remove(this.parent.getHeaderContent(), Browser.touchStartEvent, this.resizeStart);
        EventHandler.remove(this.parent.getHeaderContent(), 'dblclick', this.callAutoFit);
    }

    private getResizeHandlers(): HTMLElement[] {
        return this.parent.options.frozenColumns ?
            [].slice.call(this.parent.getHeaderContent().querySelectorAll('.' + resizeClassList.root))
            : [].slice.call(this.parent.getHeaderContent().querySelector('.e-table').querySelectorAll('.' + resizeClassList.root));
    }

    private setHandlerHeight(): void {
        const element: HTMLElement[] = [].slice.call(this.parent.getHeaderContent().querySelector('.e-table').querySelectorAll('.' + resizeClassList.suppress));
        for (let i: number = 0; i < element.length; i++) {
            const headerCellContainer : HTMLElement = element[parseInt(i.toString(), 10)].parentElement as HTMLElement;
            if (!isNullOrUndefined(headerCellContainer) && !isNullOrUndefined(headerCellContainer.parentElement)) {
                element[parseInt(i.toString(), 10)].style.height = (headerCellContainer.parentElement as HTMLElement).offsetHeight + 'px';
            }
        }
    }

    private callAutoFit(e: PointerEvent | TouchEvent): void {
        if ((e.target as HTMLElement).classList.contains('e-rhandler')) {
            const col: Column = this.getTargetColumn(e);
            if (col.columns) {
                return;
            }
            this.resizeColumn(col.field, this.parent.getNormalizedColumnIndex(col.uid), col.uid, this.isDblClk ? true : false);
            const header: HTMLElement = <HTMLElement>closest(<HTMLElement>e.target, resizeClassList.header);
            header.classList.add('e-resized');
        }
    }

    private resizeStart(e: PointerEvent | TouchEvent): void {
        if ((e.target as HTMLElement).classList.contains('e-rhandler')) {
            const columnList: Object[] = [];
            let columnData: Object = {};
            if (this.parent.isMacSafariBrowser()) {
                document.body.classList.add('e-prevent-select'); // This is being used to prevent text highlighting issues in Mac OS Safari browser.
            }
            if (!this.helper) {
                if (this.getScrollBarWidth() === 0) {
                    if (this.parent.options.allowGrouping) {
                        for (let i: number = 0; i < this.parent.options.groupCount; i++) {
                            this.widthService.setWidth('30px', i);
                        }
                    }
                    const refreshedColumns: Column[] = this.refreshColumnWidth();
                    for (const col of refreshedColumns) {
                        this.widthService.setColumnWidth(col, null, null, false);
                        columnData = { width: col.width.toString(), columnUid: col.uid };
                        columnList.push(columnData);
                    }
                    this.widthService.setWidthToTable();
                }
                this.refreshStackedColumnWidth();
                this.element = e.target as HTMLElement;
                //TODO: rowheight
                // if (this.parent.getVisibleFrozenColumns()) {
                //     let mtbody: Element = this.parent.getContent().querySelector('.e-movablecontent').querySelector('tbody');
                //     let ftbody: Element = this.parent.getContent().querySelector('.e-frozencontent').querySelector('tbody');
                //     let mtr: NodeListOf<HTMLElement> = mtbody.querySelectorAll('tr');
                //     let ftr: NodeListOf<HTMLElement> = ftbody.querySelectorAll('tr');
                //     for (let i: number = 0; i < mtr.length; i++) {
                //         if (this.parent.rowHeight) {
                //             mtr[i].style.height = this.parent.rowHeight + 'px';
                //             ftr[i].style.height = this.parent.rowHeight + 'px';
                //         } else {
                //             mtr[i].style.removeProperty('height');
                //             ftr[i].style.removeProperty('height');
                //         }
                //     }
                // }
                this.parentElementWidth = this.parent.element.getBoundingClientRect().width;
                this.appendHelper();
                this.column = this.getTargetColumn(e);
                this.pageX = this.getPointX(e);
                if (this.column.freeze === 'Right') {
                    if (this.parent.options.enableRtl) {
                        this.minMove = (this.column.minWidth ? parseFloat(this.column.minWidth.toString()) : 0)
                            - parseFloat(isNullOrUndefined(this.column.width) ? '' : this.column.width.toString());
                    } else {
                        this.minMove = parseFloat(isNullOrUndefined(this.column.width) ? '' : this.column.width.toString())
                            - (this.column.minWidth ? parseFloat(this.column.minWidth.toString()) : 0);
                    }
                } else if (this.parent.options.enableRtl) {
                    this.minMove = parseFloat(this.column.width.toString())
                        - (this.column.minWidth ? parseFloat(this.column.minWidth.toString()) : 0);
                } else {
                    this.minMove = (this.column.minWidth ? parseFloat(this.column.minWidth.toString()) : 0)
                        - parseFloat(isNullOrUndefined(this.column.width) ? '' : this.column.width.toString());
                }
                this.minMove += this.pageX;
            }
            if (Browser.isDevice && !this.helper.classList.contains(resizeClassList.icon)) {
                this.helper.classList.add(resizeClassList.icon);
                EventHandler.add(document, Browser.touchStartEvent, this.removeHelper, this);
                EventHandler.add(this.helper, Browser.touchStartEvent, this.resizeStart, this);
            } else {
                // let args: ResizeArgs = {
                //     e: isBlazor() && !this.parent.isJsComponent ? null : e,
                //     column: this.column
                // };
                // this.parent.trigger(events.resizeStart, args, (args: ResizeArgs) => {
                //     if (args.cancel || this.parent.isEdit) {
                //         this.cancelResizeAction();
                //         return;
                //     }
                EventHandler.add(document, Browser.touchEndEvent, this.resizeEnd, this);
                this.parent.dotNetRef.invokeMethodAsync('ResizeStarted', {
                    columnUid: this.column.uid, columnList: columnList
                });
                // });
            }
            if ((this.parent.options.enableVirtualization) && this.parent.options.frozenColumns) {
                (this.parent.element.querySelector('.e-virtualtable') as HTMLElement).style.position = '';
            }
        }
    }

    /**
     * Prevents or allows resize actions based on the provided flag.
     *
     * @param {boolean} isCancel - Flag indicating whether to cancel the resize action.
     * @returns {void}
     * @hidden
     */
    public preventResizeAction(isCancel: boolean): void {
        if (isCancel) {
            this.cancelResizeAction();
        } else {
            EventHandler.add(this.parent.element, Browser.touchMoveEvent, this.resizing, this);
            this.updateCursor('add');
        }
    }

    private cancelResizeAction(removeEvents?: boolean): void {
        if (removeEvents) {
            EventHandler.remove(this.parent.element, Browser.touchMoveEvent, this.resizing);
            EventHandler.remove(document, Browser.touchEndEvent, this.resizeEnd);
            this.updateCursor('remove');
        }
        if (Browser.isDevice) {
            EventHandler.remove(document, Browser.touchStartEvent, this.removeHelper);
            EventHandler.remove(this.helper, Browser.touchStartEvent, this.resizeStart);
        }
        detach(this.helper);
        this.refresh();
    }

    private getWidth(width: number, minWidth: number, maxWidth: number): number {
        if (minWidth && width < minWidth) {
            return minWidth;
        } else if ((maxWidth && width > maxWidth)) {
            return maxWidth;
        } else {
            return width;
        }
    }

    private updateResizeEleHeight(): void {
        const elements: HTMLElement[] = [].slice.call(this.parent.getHeaderContent().querySelectorAll('.e-rhandler'));
        for (let i: number = 0; i < elements.length; i++) {
            elements[parseInt(i.toString(), 10)].style.height = (elements[parseInt(i.toString(), 10)].parentElement as HTMLElement).parentElement.offsetHeight + 'px';
        }
    }

    private getColData(column: Column, mousemove: number): { [key: string]: number } {
        return {
            width: parseFloat(isNullOrUndefined(this.widthService.getWidth(column)) || this.widthService.getWidth(column) === 'auto' ? '0'
                : this.widthService.getWidth(column).toString()) + mousemove,
            minWidth: column.minWidth ? parseFloat(column.minWidth.toString()) : null,
            maxWidth: column.maxWidth ? parseFloat(column.maxWidth.toString()) : null
        };
    }

    private resizing(e: PointerEvent | TouchEvent): void {
        if (isNullOrUndefined(this.column)) {
            return;
        }
        let offsetWidth: number = 0;
        if (isNullOrUndefined(this.column)) {
            offsetWidth = (parentsUntil(this.element, 'th') as HTMLTableCellElement).offsetWidth;
        }
        if (this.parent.options.allowTextWrap) {
            this.updateResizeEleHeight();
            this.setHelperHeight();
            this.parent.scrollModule.refresh();
        }
        let pageX: number = this.getPointX(e);
        let mousemove: number = this.parent.options.enableRtl ? -(pageX - this.pageX) : (pageX - this.pageX);
        if (this.column.freeze === 'Right' && this.column.isFrozen) {
            mousemove = this.parent.options.enableRtl ? (pageX - this.pageX) : (this.pageX - pageX);
        }
        const colData: { [key: string]: number } = this.getColData(this.column, mousemove);
        if (!colData.width) {
            colData.width = (closest(this.element, 'th') as HTMLElement).offsetWidth;
        }
        let width: number = this.getWidth(colData.width, colData.minWidth, colData.maxWidth);
        if ((this.column.freeze !== 'Right' && ((!this.parent.options.enableRtl && this.minMove >= pageX - 10) || (this.parent.options.enableRtl && this.minMove <= pageX + 10)))
            || (this.column.freeze === 'Right' && ((this.parent.options.enableRtl && this.minMove >= pageX - 10) || (!this.parent.options.enableRtl && this.minMove <= pageX + 10)))) {
            width = this.column.minWidth ? parseFloat(this.column.minWidth.toString()) : 10;
            this.pageX = pageX = this.minMove;
        }
        if (width !== parseFloat(isNullOrUndefined(this.column.width) || this.column.width === 'auto' ?
            offsetWidth.toString() : this.column.width.toString())) {
            this.pageX = pageX;
            this.column.width = formatUnit(width);
            // let args: ResizeArgs = {
            //     e: e,
            //     column: this.column
            // };
            //this.parent.trigger(events.onResize, args);
            // if (args.cancel) {
            //     this.cancelResizeAction(true);
            //     return;
            // }
            let columns: Column[] = [this.column];
            let finalColumns: Column[] = [this.column];
            if (this.column.columns) {
                columns = this.getSubColumns(this.column, []);
                columns = this.calulateColumnsWidth(columns, false, mousemove);
                finalColumns = this.calulateColumnsWidth(columns, true, mousemove);
            }
            for (const col of finalColumns) {
                this.widthService.setColumnWidth(col, null, 'resize');
            }
            this.updateHelper();
        }
        this.refreshResizeFrozenColumns();
        this.isDblClk = false;
    }

    private refreshResizeFrozenColumns(): void {
        const translateX: number = this.parent.options.enableColumnVirtualization ? this.getTranslateX() : 0;
        if ((this.column.freeze === 'Left' && this.column.isFrozen) || (this.parent.options.frozenLeftColumnsCount === 0 && this.parent.options.frozenRightColumnsCount === 0 && this.column.index < this.parent.options.frozenColumns)) {
            let width: number = this.parent.getIndentCount() * 30;
            const columns: Column[] = this.parent.getColumns(true).filter((col: Column) => (col.freeze === 'Left' && col.isFrozen) || (this.parent.options.frozenLeftCount === 0 && this.parent.options.frozenRightCount === 0 && col.index < this.parent.options.frozenColumns));
            this.frozenHeaderRefresh('Left');
            for (let i: number = 0; i < columns.length; i++) {
                if (columns[parseInt(i.toString(), 10)].index > this.column.index) {
                    let elements: HTMLTableCellElement[] = [];
                    if (this.parent.options.frozenRows) {
                        elements = [].slice.call(this.parent.getHeaderContent().querySelectorAll('td[aria-colindex="' + (i + 1) + '"]')).concat([].slice.call(this.parent.getContent().querySelectorAll('td[aria-colindex="' + (i + 1) + '"]')));
                    }
                    else {
                        elements = [].slice.call(this.parent.getContent().querySelectorAll('td[aria-colindex="' + (i + 1) + '"]'));
                    }
                    elements.filter((cell: HTMLTableCellElement) => {
                        applyStickyLeftRightPosition(cell, width - translateX, this.parent.options.enableRtl, 'Left');
                    });
                    if (this.parent.options.enableColumnVirtualization) {
                        (<{ valueX?: number }>columns[parseInt(i.toString(), 10)]).valueX = width;
                    }
                }
                if (columns[parseInt(i.toString(), 10)].visible) {
                    columns[parseInt(i.toString(), 10)].translateLeftRightValue = width;
                    width += parseFloat(columns[parseInt(i.toString(), 10)].width.toString());
                }
            }
            this.refreshResizeFixedCols('Left');
        }
        if (this.column.freeze === 'Right' && this.column.isFrozen) {
            let width: number = 0;
            const columns: Column[] = this.parent.getColumns(true);
            this.frozenHeaderRefresh('Right');
            const columnsRight: Column[] = columns.filter((col: Column) => col.freeze === 'Right' && col.isFrozen);
            for (let i: number = columns.length - 1; i >= columns.length - columnsRight.length; i--) {
                let elements: HTMLTableCellElement[] = [];
                if (this.parent.options.frozenRows) {
                    elements = [].slice.call(this.parent.getHeaderContent().querySelectorAll('td[aria-colindex="' + (i + 1) + '"]')).concat([].slice.call(this.parent.getContent().querySelectorAll('td[aria-colindex="' + (i + 1) + '"]')));
                }
                else {
                    elements = [].slice.call(this.parent.getContent().querySelectorAll('td[aria-colindex="' + (i + 1) + '"]'));
                }
                elements.filter((cell: HTMLTableCellElement) => {
                    applyStickyLeftRightPosition(cell, width + translateX, this.parent.options.enableRtl, 'Right');
                });
                if (this.parent.options.enableColumnVirtualization) {
                    (<{ valueX?: number }>columns[parseInt(i.toString(), 10)]).valueX = width;
                }
                if (columns[parseInt(i.toString(), 10)].visible) {
                    columns[parseInt(i.toString(), 10)].translateLeftRightValue = width;
                    width = width + parseFloat(columns[parseInt(i.toString(), 10)].width.toString());
                }
            }
            this.refreshResizeFixedCols('Right');
        }
        if (this.column.isFrozen && this.column.freeze === 'Fixed') {
            this.refreshResizeFixedCols('Left');
            this.refreshResizeFixedCols('Right');
            this.frozenHeaderRefresh('Left');
            this.frozenHeaderRefresh('Right');
        }
    }

    private frozenHeaderRefresh(pos?: string): void {
        const translateX: number = this.parent.options.enableColumnVirtualization ? this.getTranslateX() : 0;
        if (pos === 'Left') {
            const tr: HTMLElement[] = [].slice.call(this.parent.getHeaderContent().querySelector('thead').querySelectorAll('tr'));
            for (let i: number = 0; i < tr.length; i++) {
                const th: HTMLElement[] = [].slice.call(tr[parseInt(i.toString(), 10)].querySelectorAll('.e-leftfreeze,.e-fixedfreeze'));
                for (let j: number = 0; j < th.length; j++) {
                    const node: HTMLElement = th[parseInt(j.toString(), 10)];
                    if (node.classList.contains('e-rowdragheader') || node.classList.contains('e-detailheadercell') ||
                        node.classList.contains('e-grouptopleftcell')) {
                        continue;
                    }
                    const column: Column = this.getParticularColumn(node);
                    const cols: Column[] = this.parent.getColumns(true);
                    let width: number = 0;
                    let summarycell: HTMLElement[] = [];
                    if (this.parent.options.aggregatesCount && this.parent.getFooterContent()) {
                        if (this.parent.getFooterContent().querySelectorAll('.e-summaryrow').length) {
                            const summaryRows: HTMLElement[] = [].slice.call(this.parent.getFooterContent().querySelectorAll('.e-summaryrow'));
                            summaryRows.filter((row: HTMLElement) => {
                                summarycell.push(row.querySelector('[e-mappinguid="' + column.uid + '"]'));
                            });
                        }
                        summarycell = summarycell.concat([].slice.call(this.parent.getFooterContent().querySelectorAll('[e-mappinguid="' + column.uid + '"]')));
                    }
                    if (node.classList.contains('e-fixedfreeze')) {
                        if (this.parent.getFrozenLeftColumn().length) {
                            width = this.parent.getIndentCount() * 30;
                        }
                        for (let w: number = 0; w < cols.length; w++) {
                            if (column.index > cols[parseInt(w.toString(), 10)].index) {
                                if (column.uid === cols[parseInt(w.toString(), 10)].uid) {
                                    break;
                                }
                                if ((cols[parseInt(w.toString(), 10)].freeze === 'Left' || cols[parseInt(w.toString(), 10)].freeze === 'Fixed') &&
                                    cols[parseInt(w.toString(), 10)].isFrozen) {
                                    if (cols[parseInt(w.toString(), 10)].visible) {
                                        width += parseInt(cols[parseInt(w.toString(), 10)].width.toString(), 10);
                                    }
                                }
                            }
                        }
                        if (summarycell && summarycell.length) {
                            summarycell.filter((cell: HTMLTableCellElement) => {
                                applyStickyLeftRightPosition(cell, width - translateX, this.parent.options.enableRtl, 'Left');
                            });
                        }
                        applyStickyLeftRightPosition(node, ((width === 0 ? width : width - 1) - translateX), this.parent.options.enableRtl, 'Left');
                    }
                    else {
                        width = this.parent.getIndentCount() * 30;
                        if (column.index === 0) {
                            if (summarycell && summarycell.length) {
                                summarycell.filter((cell: HTMLTableCellElement) => {
                                    applyStickyLeftRightPosition(cell, width - translateX, this.parent.options.enableRtl, 'Left');
                                });
                            }
                            applyStickyLeftRightPosition(node, width - translateX, this.parent.options.enableRtl, 'Left');
                            if (this.parent.options.enableColumnVirtualization) {
                                (<{ valueX?: number }>column).valueX = width;
                            }
                        }
                        else {
                            for (let k: number = 0; k < cols.length; k++) {
                                if (column.index < cols[parseInt(k.toString(), 10)].index ||
                                    column.uid === cols[parseInt(k.toString(), 10)].uid) {
                                    break;
                                }
                                if (cols[parseInt(k.toString(), 10)].visible) {
                                    width += parseInt(cols[parseInt(k.toString(), 10)].width.toString(), 10);
                                }
                            }
                            if (summarycell && summarycell.length) {
                                summarycell.filter((cell: HTMLTableCellElement) => {
                                    applyStickyLeftRightPosition(cell, width - translateX, this.parent.options.enableRtl, 'Left');
                                });
                            }
                            applyStickyLeftRightPosition(node, width - translateX, this.parent.options.enableRtl, 'Left');
                            if (this.parent.options.enableColumnVirtualization) {
                                (<{ valueX?: number }>column).valueX = width;
                            }
                        }
                    }
                }
            }
        }
        if (pos === 'Right') {
            const tr: HTMLElement[] = [].slice.call(this.parent.getHeaderContent().querySelector('thead').querySelectorAll('tr'));
            for (let i: number = 0; i < tr.length; i++) {
                const th: HTMLElement[] = [].slice.call(tr[parseInt(i.toString(), 10)].querySelectorAll('.e-rightfreeze, .e-fixedfreeze'));
                for (let j: number = th.length - 1; j >= 0; j--) {
                    const node: HTMLElement = th[parseInt(j.toString(), 10)];
                    const column: Column = this.getParticularColumn(node);
                    const cols: Column[] = this.parent.getColumns(true);
                    let width: number = 0;
                    let summarycell: HTMLElement[] = [];
                    if (this.parent.options.aggregatesCount && this.parent.getFooterContent()) {
                        if (this.parent.getContent().querySelectorAll('.e-summaryrow').length) {
                            const summaryRows: HTMLElement[] = [].slice.call(this.parent.getContent().querySelectorAll('.e-summaryrow'));
                            summaryRows.filter((row: HTMLElement) => {
                                summarycell.push(row.querySelector('[e-mappinguid="' + column.uid + '"]'));
                            });
                        }
                        summarycell = summarycell.concat([].slice.call(this.parent.getFooterContent().querySelectorAll('[e-mappinguid="' + column.uid + '"]')));
                    }
                    if (node.classList.contains('e-fixedfreeze')) {
                        width = 0;
                        for (let w: number = cols.length - 1; w >= 0; w--) {
                            if (column.index < cols[parseInt(w.toString(), 10)].index) {
                                if ((cols[parseInt(w.toString(), 10)].freeze === 'Right' ||
                                    cols[parseInt(w.toString(), 10)].freeze === 'Fixed') && cols[parseInt(w.toString(), 10)].isFrozen) {
                                    if (cols[parseInt(w.toString(), 10)].visible) {
                                        width += parseFloat(cols[parseInt(w.toString(), 10)].width.toString());
                                    }
                                }
                            }
                        }
                        if (summarycell.length) {
                            summarycell.filter((cell: HTMLTableCellElement) => {
                                applyStickyLeftRightPosition(cell, width + translateX, this.parent.options.enableRtl, 'Right');
                            });
                        }
                        applyStickyLeftRightPosition(node, width + translateX, this.parent.options.enableRtl, 'Right');
                    }
                    else {
                        width = 0;
                        for (let k: number = cols.length - 1; k >= 0; k--) {
                            if (column.index > cols[parseInt(k.toString(), 10)].index ||
                                column.uid === cols[parseInt(k.toString(), 10)].uid) {
                                break;
                            }
                            if (cols[parseInt(k.toString(), 10)].visible) {
                                width += parseInt(cols[parseInt(k.toString(), 10)].width.toString(), 10);
                            }
                        }
                        if (summarycell.length) {
                            summarycell.filter((cell: HTMLTableCellElement) => {
                                applyStickyLeftRightPosition(cell, width + translateX, this.parent.options.enableRtl, 'Right');
                            });
                        }
                        applyStickyLeftRightPosition(node, width + translateX, this.parent.options.enableRtl, 'Right');
                        if (this.parent.options.enableColumnVirtualization) {
                            (<{ valueX?: number }>column).valueX = width;
                        }
                    }
                }
            }
        }
    }

    private refreshResizeFixedCols(pos?: string): void {
        const cols: Column[] = this.parent.getColumns(true);
        const translateX: number = this.parent.options.enableColumnVirtualization ? this.getTranslateX() : 0;
        const th: HTMLTableCellElement[] = [].slice.call(this.parent.getHeaderContent().querySelector('tbody').querySelectorAll('.e-fixedfreeze')).concat(
            [].slice.call(this.parent.getContent().querySelectorAll('.e-fixedfreeze')));
        for (let i: number = 0; i < th.length; i++) {
            const node: HTMLTableCellElement = th[parseInt(i.toString(), 10)];
            let column: Column;
            if (node.classList.contains('e-summarycell')) {
                const uid: string = node.getAttribute('e-mappinguid');
                column = this.parent.getColumnByUid(uid);
            }
            else {
                const index: number = parseInt(node.getAttribute('aria-colindex'), 10) - 1;
                column = cols[parseInt(index.toString(), 10)];
            }
            let width: number = 0;
            if (pos === 'Left') {
                if (this.parent.getFrozenLeftColumn().length) {
                    width = this.parent.getIndentCount() * 30;
                }
                for (let j: number = 0; j < cols.length; j++) {
                    if (column.index > cols[parseInt(j.toString(), 10)].index) {
                        if (column.uid === cols[parseInt(j.toString(), 10)].uid) {
                            break;
                        }
                        if ((cols[parseInt(j.toString(), 10)].freeze === 'Left' || cols[parseInt(j.toString(), 10)].freeze === 'Fixed') &&
                            cols[parseInt(j.toString(), 10)].isFrozen) {
                            if (cols[parseInt(j.toString(), 10)].visible) {
                                width += parseFloat(cols[parseInt(j.toString(), 10)].width.toString());
                            }
                        }
                    }
                }
                applyStickyLeftRightPosition(node, ((width === 0 ? width : width - 1) - translateX), this.parent.options.enableRtl, 'Left');
            }
            if (pos === 'Right') {
                width = 0;
                for (let j: number = cols.length - 1; j >= 0; j--) {
                    if (column.uid === cols[parseInt(j.toString(), 10)].uid) {
                        break;
                    }
                    if ((cols[parseInt(j.toString(), 10)].freeze === 'Right' || cols[parseInt(j.toString(), 10)].freeze === 'Fixed') && cols[parseInt(j.toString(), 10)].isFrozen) {
                        if (cols[parseInt(j.toString(), 10)].visible) {
                            width += parseFloat(cols[parseInt(j.toString(), 10)].width.toString());
                        }
                    }
                }
                applyStickyLeftRightPosition(node, width + translateX, this.parent.options.enableRtl, 'Right');
            }
        }
    }

    private getParticularColumn(node?: HTMLElement): Column {
        const uid: string = node.classList.contains('e-filterbarcell') ? node.getAttribute('e-mappinguid') :
            node.querySelector('[e-mappinguid]').getAttribute('e-mappinguid');
        return this.parent.getColumnByUid(uid);
    }

    private getTranslateX(): number {
        if (!this.parent.options.enableColumnVirtualization) {
            return 0;
        }
        const element: HTMLElement = this.parent.getContent().getElementsByClassName('e-virtualtable')[0] as HTMLElement;
        const startIndex: number = element.style.transform.indexOf('(') + 1;
        const endIndex: number = element.style.transform.indexOf('p');
        const translateXValue: number = parseInt(element.style.transform.slice(startIndex, endIndex), 10);

        return translateXValue;
    }

    private calulateColumnsWidth(columns: Column[], isUpdate: boolean, mousemove: number): Column[] {
        const finalColumns: Column[] = [];
        for (const col of columns) {
            let totalWidth: number = 0;
            for (let i: number = 0; i < columns.length; i++) {
                totalWidth += parseFloat(columns[parseInt(i.toString(), 10)].width.toString());
            }
            const colData: { [key: string]: number } = this.getColData(col, (parseFloat(col.width as string)) * mousemove / totalWidth);
            const colWidth: number = this.getWidth(colData.width, colData.minWidth, colData.maxWidth);
            if ((colWidth !== parseFloat(col.width.toString()))) {
                if (isUpdate) {
                    col.width = formatUnit(colWidth < 1 ? 1 : colWidth);
                }
                finalColumns.push(col);
            }
        }
        return finalColumns;
    }

    private getSubColumns(column: Column, subColumns: Column[]): Column[] {
        for (const col of column.columns as Column[]) {
            if (col.visible !== false && col.allowResizing) {
                if (col.columns) {
                    this.getSubColumns(col, subColumns);
                } else {
                    subColumns.push(col);
                }
            }
        }
        return subColumns;
    }

    private resizeEnd(e: PointerEvent): void {
        if (!this.helper) { return; }
        if (this.parent.isMacSafariBrowser()) {
            document.body.classList.remove('e-prevent-select');
        }
        EventHandler.remove(this.parent.element, Browser.touchMoveEvent, this.resizing);
        EventHandler.remove(document, Browser.touchEndEvent, this.resizeEnd);
        this.updateCursor('remove');
        detach(this.helper);
        // let args: ResizeArgs = {
        //     e: isBlazor() && !this.parent.isJsComponent ? null : e,
        //     column: this.column
        // };
        // let cTable: HTMLElement = content.querySelector('.e-movablecontent') ? content.querySelector('.e-movablecontent') : content;
        // if (cTable.scrollHeight >= cTable.clientHeight) {
        //     this.parent.scrollModule.setPadding();
        //     cTable.style.overflowY = 'scroll';
        // }
        //this.parent.trigger(events.resizeStop, args);
        closest(this.element, '.e-headercell').classList.add('e-resized');
        if (parentsUntil(this.element, 'e-frozenheader')) {
            this.isFrozenColResized = true;
        } else {
            this.isFrozenColResized = false;
        }
        if (this.parent.options.frozenColumns) {
            this.parent.freezeModule.refreshRowHeight();
            this.parent.freezeModule.setFrozenHeight();
        }

        if (this.parent.options.allowTextWrap) {
            this.updateResizeEleHeight();
        }
        let width: string = this.column.width.toString();
        width = width.replace('px', '');
        this.parent.dotNetRef.invokeMethodAsync('ColumnWidthChanged', {
            width: width, columnUid: this.column.uid, allowStopEvent: !this.isDblClk,
            tableWidth: this.tableWidth, leftFrozenTableWidth: this.leftFrozenTableWidth,
            rightFrozenTableWidth: this.rightFrozenTableWidth
        }, false);
        if (!this.parent.options.enableColumnVirtualization) {
            this.parent.updateColumnWidth(this.parent.columnModel);
        }
        this.parent.addTableBorderClass();
        this.refresh();
        this.doubleTapEvent(e);
        this.isDblClk = true;
    }

    private getPointX(e: PointerEvent | TouchEvent): number {
        if ((e as TouchEvent).touches && (e as TouchEvent).touches.length) {
            return (e as TouchEvent).touches[0].pageX;
        } else {
            return (e as PointerEvent).pageX;
        }
    }

    private refreshColumnWidth(): Column[] {
        const columns: Column[] = this.parent.getColumns();
        const headerCells: HTMLElement[] = [].slice.apply(this.parent.getHeaderContent().querySelectorAll('th.e-headercell'));
        for (const ele of headerCells) {
            for (const column of columns) {
                if (ele.querySelector('[e-mappinguid]') &&
                    ele.querySelector('[e-mappinguid]').getAttribute('e-mappinguid') === column.uid && column.visible) {
                    column.width = ele.getBoundingClientRect().width ? ele.getBoundingClientRect().width : column.width;
                    break;
                }
                if (!column.visible && typeof (column.width) === 'string' && (column.width as string).includes('px')) {
                    column.width = Number(column.width.replace('px', ''));
                }
            }
        }
        return columns;
    }

    private refreshStackedColumnWidth(): void {
        for (const stackedColumn of this.parent.getStackedColumns(this.parent.options.columns as Column[])) {
            stackedColumn.width = this.getStackedWidth(stackedColumn, 0);
        }
    }

    private getStackedWidth(column: Column, width: number): number {
        for (const col of column.columns as Column[]) {
            if (col.visible !== false) {
                if (col.columns) {
                    width += this.getStackedWidth(col, 0);
                } else {
                    width += col.width as number;
                }
            }
        }
        return width;
    }

    private getTargetColumn(e: PointerEvent | TouchEvent): Column {
        let cell: HTMLElement = <HTMLElement>closest(<HTMLElement>e.target, resizeClassList.header);
        cell = cell.querySelector('.e-headercelldiv') || cell.querySelector('.e-stackedheadercelldiv');
        const uid: string = cell.getAttribute('e-mappinguid');
        return this.parent.getColumnByUid(uid);
    }

    private updateCursor(action: string): void {
        const headerRows: Element[] = [].slice.call(this.parent.getHeaderContent().querySelectorAll('th'));
        headerRows.push(this.parent.element);
        for (const row of headerRows) {
            row.classList[`${action}`](resizeClassList.cursor);
        }
    }

    private refresh(): void {
        this.column = null;
        this.pageX = null;
        this.element = null;
        this.helper = null;
    }

    private appendHelper(): void {
        this.helper = createElement('div', {
            className: resizeClassList.helper
        });
        this.parent.element.appendChild(this.helper);
        this.setHelperHeight();
    }

    private setHelperHeight(): void {
        let height: number = (<HTMLElement>this.parent.getContent()).offsetHeight - ((this.parent.options.frozenColumns) ?
            0 : this.getScrollBarWidth());
        const rect: HTMLElement = closest(this.element, resizeClassList.header) as HTMLElement;
        const tr: HTMLElement[] = [].slice.call(this.parent.getHeaderContent().querySelectorAll('tr'));
        for (let i: number = tr.indexOf(rect.parentElement); i < tr.length; i++) {
            height += tr[parseInt(i.toString(), 10)].offsetHeight;
        }
        const pos: OffsetPosition = this.calcPos(rect);
        if (parentsUntil(rect, 'e-frozen-right-header')) {
            pos.left += (this.parent.options.enableRtl ? rect.offsetWidth - 2 : 0 - 1);
        }
        else {
            pos.left += (this.parent.options.enableRtl ? 0 - 1 : rect.offsetWidth - 2);
        }
        this.helper.style.cssText = 'height: ' + height + 'px; top: ' + pos.top + 'px; left:' + Math.floor(pos.left) + 'px;';
    }

    private getScrollBarWidth(height?: boolean): number {
        const ele: HTMLElement = this.parent.getContent() as HTMLElement;
        return (ele.scrollHeight > ele.clientHeight && height) ||
            ele.scrollWidth > ele.clientWidth ? getScrollBarWidth() : 0;
    }

    private removeHelper(e: MouseEvent): void {
        const cls: DOMTokenList = (e.target as HTMLElement).classList;
        if (!(cls.contains(resizeClassList.root) || cls.contains(resizeClassList.icon)) && this.helper) {
            EventHandler.remove(document, Browser.touchStartEvent, this.removeHelper);
            EventHandler.remove(this.helper, Browser.touchStartEvent, this.resizeStart);
            detach(this.helper);
            this.refresh();
        }
    }

    private updateHelper(): void {
        const rect: HTMLElement = closest(this.element, resizeClassList.header) as HTMLElement;
        if (isNullOrUndefined(rect)) {
            return;
        }
        let left: number = Math.floor(this.calcPos(rect).left + (this.parent.options.enableRtl ? 0 - 1 : rect.offsetWidth - 2));
        const borderWidth: number = 2; // to maintain the helper inside of grid element.
        if (parentsUntil(rect, 'e-frozen-right-header')) {
            left = Math.floor(this.calcPos(rect).left + (this.parent.options.enableRtl ? rect.offsetWidth - 2 : 0 - 1));
        }
        if (left > this.parentElementWidth) {
            left = this.parentElementWidth - borderWidth;
        }
        if (this.parent.options.frozenColumns) {
            const table: HTMLElement = closest(rect, '.e-table') as HTMLElement;
            const fLeft: number = table.offsetLeft;
            if (left < fLeft) {
                left = fLeft;
            }
        }
        this.helper.style.left = left + 'px';
    }

    private calcPos(elem: HTMLElement): OffsetPosition {
        let parentOffset: OffsetPosition = {
            top: 0,
            left: 0
        };
        if (isNullOrUndefined(elem)) {
            return parentOffset;
        }
        const offset: OffsetPosition = elem.getBoundingClientRect();
        const doc: Document = elem.ownerDocument;
        let offsetParent: Node = parentsUntil(elem, 'e-grid') || doc.documentElement;
        while (offsetParent &&
            (offsetParent === doc.body || offsetParent === doc.documentElement) &&
            (<HTMLElement>offsetParent).style.position === 'static') {
            offsetParent = offsetParent.parentNode;
        }
        if (offsetParent && offsetParent !== elem && offsetParent.nodeType === 1) {
            parentOffset = (<HTMLElement>offsetParent).getBoundingClientRect();
        }
        return {
            top: offset.top - parentOffset.top,
            left: offset.left - parentOffset.left
        };
    }

    private doubleTapEvent(e: TouchEvent | PointerEvent): void {
        if (this.getUserAgent() && this.isDblClk) {
            if (!this.tapped) {
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                this.tapped = <number><any>setTimeout(this.timeoutHandler.bind(this), 300);
            } else {
                clearTimeout(this.tapped as number);
                this.callAutoFit(e);
                this.tapped = null;
            }
        }
    }

    private getUserAgent(): boolean {
        const userAgent: string = Browser.userAgent.toLowerCase();
        return (/iphone|ipod|ipad/ as RegExp).test(userAgent);
    }

    private timeoutHandler(): void {
        this.tapped = null;
    }
}
