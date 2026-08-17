import { isNullOrUndefined, createElement, KeyboardEventArgs } from '@syncfusion/ej2-base';
import { remove, removeClass, closest as closestElement, classList, EventHandler} from '@syncfusion/ej2-base';
import { SfGrid } from './sf-grid-fn';
import { Column } from './interfaces';
import { parentsUntil, getElementIndex, getPosition, inArray, isActionPrevent } from './util';
/**
 * Column reorder handling
 */
export class Reorder {
    private parent: SfGrid;
    private element: HTMLElement;
    private upArrow: HTMLElement;
    private downArrow: HTMLElement;
    private x: number;
    private timer: number;
    private destElement: Element;
    private fromCol: string;
    private draggedHeader: HTMLElement;

    constructor(parent: SfGrid) {
        this.parent = parent;
        if (parent.options.allowReordering) {
            this.createReorderElement();
        }
    }

    private chkDropPosition(srcElem: Element, destElem: Element): boolean {
        if (isNullOrUndefined(srcElem.parentElement)){
            return false;
        }
        const mappingElement: HTMLElement = destElem.querySelector('[e-mappinguid]');
        const mappingUid: string = !isNullOrUndefined(mappingElement) ? mappingElement.getAttribute('e-mappinguid') : '';
        const col: Column = this.parent.getColumnByUid(mappingUid);
        const bool: boolean = col ? !col.fixedColumn : true;
        return (srcElem.parentElement.isEqualNode(destElem.parentElement) || (this.parent.options.frozenColumns
            && Array.prototype.indexOf.call(closestElement(srcElem, 'thead').children, srcElem.parentElement)
            === Array.prototype.indexOf.call(closestElement(destElem, 'thead').children, destElem.parentElement)))
            && this.targetParentContainerIndex(srcElem, destElem) > -1 && bool;
    }

    private chkDropAllCols(srcElem: Element, destElem: Element): boolean {
        let isFound: boolean;
        const headers: Element[] = this.getHeaderCells();
        let header: Element;
        while (!isFound && headers.length > 0) {
            header = headers.pop();
            isFound = srcElem !== header && this.targetParentContainerIndex(srcElem, destElem) > -1;
        }
        return isFound;
    }

    private findColParent(col: Column, cols: Column[], parent: Column[]): boolean {
        for (let i: number = 0, len: number = cols.length; i < len; i++) {
            if (col === cols[parseInt(i.toString(), 10)]) {
                return true;
            } else if (cols[parseInt(i.toString(), 10)].columns) {
                const cnt: number = parent.length;
                parent.push(cols[parseInt(i.toString(), 10)]);
                if (!this.findColParent(col, cols[parseInt(i.toString(), 10)].columns as Column[], parent)) {
                    parent.splice(cnt, parent.length - cnt);
                } else {
                    return true;
                }
            }
        }
        return false;
    }

    public getColumnsModel(cols: Column[]): Column[] {
        let columnModel: Column[] = [];
        let subCols: Column[] = [];
        for (let i: number = 0, len: number = cols.length; i < len; i++) {
            columnModel.push(cols[parseInt(i.toString(), 10)]);
            if (cols[parseInt(i.toString(), 10)].columns) {
                subCols = subCols.concat(cols[parseInt(i.toString(), 10)].columns as Column[]);
            }
        }
        if (subCols.length) {
            columnModel = columnModel.concat(this.getColumnsModel(subCols as Column[]));
        }
        return columnModel;
    }

    public headerDrop(e: { target: Element }): void {
        if (isNullOrUndefined(this.element)) {
            return;
        }

        const gObj: SfGrid = this.parent;
        let dropElement: Element = this.element.querySelector('.e-headercelldiv') || this.element.querySelector('.e-stackedheadercelldiv');
        if (this.parent.options.enableColumnVirtualization) {
            dropElement = this.draggedHeader.querySelector('.e-headercelldiv') || this.draggedHeader.querySelector('.e-stackedheadercelldiv');
        }
        const uId: string = dropElement.getAttribute('e-mappinguid');
        const column: Column = gObj.getColumnByUid(uId);
        let isStackedColumnLocked: boolean = false;
        if (!isNullOrUndefined(this.draggedHeader.querySelector('.e-stackedheadercelldiv'))) {
            const headerCells: Element[] = this.getHeaderCells();
            const stackedHdrColumn: Column[] = this.parent.getStackedColumns(this.parent.options.columns);
            const stackedCols: Column[] = stackedHdrColumn.length > 0 ? this.getAllStackedheaderParentColumns(headerCells) : [];
            const columnsList: Column[] = this.processStackedColumns(this.parent.options.columns);
            const columnsOrder: Column[] = (stackedHdrColumn.length > 0 && stackedCols.length > 0) ? columnsList :
                this.parent.options.columns;
            const flatColumns: Column[] = this.getColumnsModel(columnsOrder);
            if (!isNullOrUndefined(dropElement.parentElement) &&
                !isNullOrUndefined((dropElement.parentElement as HTMLElement).parentElement)) {
                const thElement: Element = (dropElement.parentElement as HTMLElement).parentElement as Element;
                isStackedColumnLocked = this.stackedLockedColumn(flatColumns, thElement);
            }
        }
        if (!closestElement(e.target, 'th') || (!isNullOrUndefined(column) && (!column.allowReordering || column.fixedColumn)) || isStackedColumnLocked) {
            return;
        }
        const destElem: Element = closestElement(e.target as Element, '.e-headercell');
        if (isNullOrUndefined(destElem)) {
            return;
        }
        const destElemDiv: Element = destElem.querySelector('.e-headercelldiv') || destElem.querySelector('.e-stackedheadercelldiv');
        const destElemUid: string = destElemDiv.getAttribute('e-mappinguid');
        if (!isNullOrUndefined(destElemUid)) {
            const destColumn: Column = gObj.getColumnByUid(destElemUid);
            if (isNullOrUndefined(destColumn) || !destColumn.allowReordering || destColumn.fixedColumn) {
                return;
            }
        }
        if (destElem && !(!this.chkDropPosition(this.element, destElem) || !this.chkDropAllCols(this.element, destElem))) {
            if (this.parent.options.enableColumnVirtualization) {
                const columns: Column[] = this.parent.options.columns as Column[];
                const sourceUid: string = this.draggedHeader.querySelector('.e-headercelldiv').getAttribute('e-mappinguid');
                const col: Column[] = this.parent.getColumns(false).filter((col: Column) => col.uid === sourceUid);
                let colMatchIndex: number = null;
                const column: Column = col[0];
                const destUid: string = destElem.querySelector('.e-headercelldiv').getAttribute('e-mappinguid');
                columns.some((col: Column, index: number) => {
                    if (col.uid === destUid) {
                        colMatchIndex = index;
                        return col.uid === destUid;
                    }
                    return false;
                });
                if (!isNullOrUndefined(colMatchIndex)) {
                    this.moveColumns(colMatchIndex, column);
                }
            } else {
                const newIndex: number = this.targetParentContainerIndex(this.element, destElem);
                const mappingElement: HTMLElement = this.element.querySelector('[e-mappinguid]');
                const uid: string = !isNullOrUndefined(mappingElement) ? mappingElement.getAttribute('e-mappinguid') : '';
                this.destElement = destElem;
                if (uid) {
                    this.moveColumns(newIndex, this.parent.getColumnByUid(uid));
                } else {
                    const headers: Element[] = this.getHeaderCells();
                    const oldIdx: number = getElementIndex(this.element, headers);
                    const columns: Column[] = this.getColumnsModel(this.parent.options.columns as Column[]);
                    const column: Column = columns[parseInt(oldIdx.toString(), 10)];
                    this.moveColumns(newIndex, column);
                }
            }
        }
    }

    private isActionPrevent(gObj: SfGrid): boolean {
        return isActionPrevent(gObj.element, gObj.options.editMode);
    }

    private moveColumns(destIndex: number, column: Column, reorderByColumn?: boolean, preventRefresh?: boolean): void {
        const gObj: SfGrid = this.parent;
        if (this.isActionPrevent(gObj)) {
            //gObj.notify(events.preventBatch, { instance: this, handler: this.moveColumns, arg1: destIndex, arg2: column });
            return;
        }
        const parent: Column = this.getColParent(column, this.parent.options.columns as Column[]);
        const cols: Column[] = parent ? parent.columns as Column[] : this.parent.options.columns as Column[];
        let srcIdx: number = inArray(column, cols);
        if (((this.parent.options.frozenColumns && parent) || this.parent.checkFixedColumns(this.parent.options.columns))
            && !reorderByColumn) {
            for (let i: number = 0; i < cols.length; i++) {
                const gridcolumn: Column = cols[parseInt(i.toString(), 10)];
                if (gridcolumn.field === column.field && gridcolumn.index === column.index) {
                    srcIdx = i;
                    break;
                }
            }
            const mappingElement: HTMLElement = this.destElement.querySelector('[e-mappinguid]');
            const mappingUid: string = !isNullOrUndefined(mappingElement) ? mappingElement.getAttribute('e-mappinguid') : '';
            const col: Column = this.parent.getColumnByUid(mappingUid);
            if (col) {
                for (let i: number = 0; i < cols.length; i++) {
                    const gridcolumn: Column = cols[parseInt(i.toString(), 10)];
                    if (gridcolumn.field === col.field && gridcolumn.index === col.index) {
                        destIndex = i;
                        break;
                    }
                }
            } else {
                for (let i: number = 0; i < cols.length; i++) {
                    const gridcolumn: Column = cols[parseInt(i.toString(), 10)];
                    if (gridcolumn.headerText === (this.destElement as HTMLElement).innerText.trim()) {
                        destIndex = i;
                    }
                }
            }
            const destStackedHeader: boolean = this.destElement.classList.contains('e-stackedheadercell');
            const srcStackedHeader: boolean = this.element.classList.contains('e-stackedheadercell');
            if ((destStackedHeader && srcStackedHeader) && (this.isAnyColumnFixed(cols[parseInt(destIndex.toString(), 10)])
                || this.isAnyColumnFixed(cols[parseInt(srcIdx.toString(), 10)]))) {
                return;
            }
        }
        if (!gObj.options.allowReordering || srcIdx === destIndex || srcIdx === -1 || destIndex === -1) {
            return;
        }
        (cols as Column[]).splice(destIndex, 0, (cols as Column[]).splice(srcIdx, 1)[0] as Column);
        gObj.getColumns(true);
        //gObj.notify(events.columnPositionChanged, { fromIndex: destIndex, toIndex: srcIdx });
        if (preventRefresh !== false) {
            //TODO: reorder from here
            setTimeout(() => {
                gObj.dotNetRef.invokeMethodAsync('ColumnReordered', {
                    requestType: 'reorder', fromIndex: destIndex, toIndex: srcIdx, toColumnUid: column.uid
                });
            }, 10);
        }
    }

    private targetParentContainerIndex(srcElem: Element, destElem: Element): number {
        const headers: Element[] = this.getHeaderCells();
        if (this.parent.options.frozenName !== 'None') {
            this.parent.updateColumnLevelFrozen();
        }
        let cols: Column[] = this.parent.options.columns as Column[];
        const lockedOrderedColumns: Column[] = this.processStackedColumns(cols);
        const flatColumns: Column[] = this.parent.options.frozenName === 'None' ?
            this.getColumnsModel(lockedOrderedColumns) : this.parent.frozenColumnModel.slice();
        const parent: Column = this.getColParent(flatColumns[getElementIndex(srcElem, headers)], cols);
        cols = parent ? parent.columns as Column[] : lockedOrderedColumns;
        if (srcElem.classList.contains('e-stackedheadercell') && destElem.classList.contains('e-stackedheadercell')) {
            const srcMappingElement: HTMLElement = srcElem.querySelector('[e-mappinguid]');
            const srcElemntUID: string = !isNullOrUndefined(srcMappingElement) ? srcMappingElement.getAttribute('e-mappinguid') : '';
            const destMappingElement: HTMLElement = srcElem.querySelector('[e-mappinguid]');
            const destElementUID: string = !isNullOrUndefined(destMappingElement) ? destMappingElement.getAttribute('e-mappinguid') : '';
            const sourceColumn: Column = cols.filter((col: Column) => col.uid === srcElemntUID)[0];
            const destColumn: Column = cols.filter((col: Column) => col.uid === destElementUID)[0];
            if (this.isAnyColumnFixed(sourceColumn) || this.isAnyColumnFixed(destColumn)) {
                return -1;
            }
        }
        return inArray(flatColumns[getElementIndex(destElem, headers)], cols);
    }

    private getHeaderCells(): Element[] {
        return [].slice.call(this.parent.element.getElementsByClassName('e-headercell'));
    }

    private getColParent(column: Column, columns: Column[]): Column {
        const parents: Column[] = [];
        this.findColParent(column, columns, parents);
        return parents[parents.length - 1];
    }

    private reorderSingleColumn(fromFName: string, toFName: string): void {
        const fColumn: Column = this.parent.getColumnByField(fromFName);
        const toColumn: Column = this.parent.getColumnByField(toFName);
        if ((!isNullOrUndefined(fColumn) && (!fColumn.allowReordering || fColumn.fixedColumn)) ||
            (!isNullOrUndefined(toColumn) && (!toColumn.allowReordering || toColumn.fixedColumn))) {
            return;
        }
        const column: Column = this.parent.getColumnByField(toFName);
        const parent: Column = this.getColParent(column, this.parent.options.columns as Column[]);
        const columns: Column[] = parent ? parent.columns as Column[] : this.parent.options.columns as Column[];
        const destIndex: number = inArray(column, columns);
        if (destIndex > -1) {
            this.moveColumns(destIndex, this.parent.getColumnByField(fromFName), true);
        }
    }

    private reorderMultipleColumns(fromFNames: string[], toFName: string): void {
        let toIndex: number = this.parent.getColumnIndexByField(toFName);
        const toColumn: Column = this.parent.getColumnByField(toFName);
        if (toIndex < 0 || (!isNullOrUndefined(toColumn) && (!toColumn.allowReordering || toColumn.fixedColumn))) {
            return;
        }
        for (let i: number = 0; i < fromFNames.length; i++) {
            const column: Column = this.parent.getColumnByField(fromFNames[parseInt(i.toString(), 10)]);
            if (!isNullOrUndefined(column) && (!column.allowReordering || column.fixedColumn)) {
                return;
            }
        }
        for (let i: number = 0; i < fromFNames.length; i++) {
            const column: Column = this.parent.getColumnByIndex(toIndex);
            const parent: Column = this.getColParent(column, this.parent.options.columns as Column[]);
            const columns: Column[] = parent ? parent.columns as Column[] : this.parent.options.columns as Column[];
            const destIndex: number = inArray(column, columns);
            if (destIndex > -1) {
                this.moveColumns(
                    destIndex, this.parent.getColumnByField(fromFNames[parseInt(i.toString(), 10)]), true, false);
            }
            if (this.parent.getColumnIndexByField(fromFNames[i + 1]) >= destIndex) {
                toIndex++; //R to L
            }
        }

        const cols: Column[] = this.parent.getColumns();
        this.parent.dotNetRef.invokeMethodAsync('ColumnReordered', {
            fromColumnUid: fromFNames.map((name: string) => cols.filter((col: Column) => col.field === name)[0].uid),
            toColumnUid: toColumn.uid,
            isMultipleReorder: true,
            requestType: 'reorder',
            type: 'actionBegin'
        });
    }

    private moveTargetColumn(column: Column, toIndex: number) : void {
        if (toIndex > -1) {
            this.moveColumns(toIndex, column, true);
        }
    }

    private reorderSingleColumnByTarget(fieldName: string, toIndex: number): void {
        const column: Column = this.parent.getColumnByField(fieldName);
        this.moveTargetColumn(column, toIndex);
    }

    private reorderMultipleColumnByTarget(fieldName: string[], toIndex: number): void {
        for (let i: number = 0; i < fieldName.length; i++) {
            this.reorderSingleColumnByTarget(fieldName[parseInt(i.toString(), 10)], toIndex);
        }
    }

    /**
     * Changes the position of the Grid columns by field names.
     *
     * @param  {string | string[]} fromFName - Defines the origin field names.
     * @param  {string} toFName - Defines the destination field name.
     * @returns {void}
     */
    public reorderColumns(fromFName: string | string[], toFName: string): void {
        if (typeof fromFName === 'string') {
            this.reorderSingleColumn(fromFName, toFName);
            this.fromCol = fromFName;
        } else {
            this.reorderMultipleColumns(fromFName, toFName);
            this.fromCol = fromFName[0];
        }
    }

    /**
     * Changes the position of the Grid columns by field index.
     *
     * @param  {number} fromIndex - Defines the origin field index.
     * @param  {number} toIndex - Defines the destination field index.
     * @returns {void}
     */
    public reorderColumnByIndex(fromIndex: number, toIndex: number): void {
        const column: Column = this.parent.getColumnByIndex(fromIndex);
        this.moveTargetColumn(column, toIndex);
    }

    /**
     * Changes the position of the Grid columns by field index.
     *
     * @param  {string | string[]} fieldName - Defines the field name.
     * @param  {number} toIndex - Defines the destination field index.
     * @returns {void}
     */
    public reorderColumnByTargetIndex(fieldName: string | string[], toIndex: number): void {
        if (typeof fieldName === 'string') {
            this.reorderSingleColumnByTarget(fieldName, toIndex);
        } else {
            this.reorderMultipleColumnByTarget(fieldName, toIndex);
        }
    }

    public createReorderElement(): void {
        const header: Element = (this.parent.element.querySelector('.e-headercontent') as Element);
        this.upArrow = header.appendChild(createElement('div', { className: 'e-icons e-icon-reorderuparrow e-reorderuparrow', styles: 'display:none' }));
        this.downArrow = header.appendChild(createElement('div', { className: 'e-icons e-icon-reorderdownarrow e-reorderdownarrow', styles: 'display:none' }));
        if (this.parent.options.enableColumnVirtualization) {
            this.upArrow.classList.replace('e-reorderuparrow', 'e-reorderuparrow-virtual');
            this.downArrow.classList.replace('e-reorderdownarrow', 'e-reorderdownarrow-virtual');
        }
    }

    /**
     * The function used to trigger onActionComplete
     *
     * @return {void}
     * @hidden
     */
    // public onActionComplete(e: NotifyArgs): void {
    //     if (isBlazor() && !this.parent.isJsComponent) {
    //         e.rows = null;
    //     }
    //     this.parent.trigger(events.actionComplete, extend(e, { type: events.actionComplete }));
    //     let target: Element = this.fromCol && this.parent.getColumnHeaderByField(this.fromCol);
    //     if (target) {
    //         this.parent.focusModule.onClick({ target }, true);
    //     }
    // }

    /**
     * To destroy the reorder
     *
     * @returns {void}
     * @hidden
     */
    public destroy(): void {
        if (this.upArrow) {
            remove(this.upArrow);
        }
        if (this.downArrow) {
            remove(this.downArrow);
        }
        //call ejdrag and drop destroy
    }

    private keyPressHandler(e: KeyboardEventArgs): void {
        switch (e.action) {
        case 'ctrlLeftArrow':
        case 'ctrlRightArrow':
            // let element: HTMLElement = gObj.focusModule.currentInfo.element;
            // if (element && element.classList.contains('e-headercell')) {
            //     let column: Column = gObj.getColumnByUid(element.firstElementChild.getAttribute('e-mappinguid'));
            //     let visibleCols: Column[] = gObj.getVisibleColumns();
            //     let index: number = visibleCols.indexOf(column);
            //     let toCol: Column = e.action === 'ctrlLeftArrow' ? visibleCols[index - 1] : visibleCols[index + 1];
            //     if (toCol && toCol.field && column.field) {
            //         this.reorderColumns(column.field, toCol.field);
            //     }
            // }
            break;
        }
    }

    public drag(e: { target: Element, column: Column, event: MouseEvent }): void {
        const gObj: SfGrid = this.parent;
        let target: Element = e.target as Element;
        const cloneElement: HTMLElement = gObj.element.querySelector('.e-cloneproperties') as HTMLElement;
        if (!e.column.allowReordering || e.column.fixedColumn) {
            classList(cloneElement, ['e-notallowedcur'], ['e-defaultcur']);
            return;
        }
        const closest: Element = closestElement(target, '.e-headercell:not(.e-stackedHeaderCell)');
        const isLeft: boolean = this.x > getPosition(e.event).x + gObj.getContent().firstElementChild.scrollLeft;
        removeClass(gObj.element.querySelector('.e-headercontent').querySelectorAll('.e-reorderindicate'), ['e-reorderindicate']);
        this.setDisplay('none');
        this.stopTimer();
        classList(cloneElement, ['e-defaultcur'], ['e-notallowedcur']);
        this.updateScrollPostion(e.event);
        if (closest && !closest.isEqualNode(this.element)) {
            target = closest;
            //consider stacked, detail header cell
            const dropElement: Element = closest.querySelector('.e-headercelldiv') || closest.querySelector('.e-stackedheadercelldiv');
            const uID: string = dropElement.getAttribute('e-mappinguid');
            const column: Column = gObj.getColumnByUid(uID);
            if (!(!this.chkDropPosition(this.element, target) || !this.chkDropAllCols(this.element, target)) && column.allowReordering) {
                this.updateArrowPosition(target, isLeft);
                classList(target, ['e-allowDrop', 'e-reorderindicate'], []);
            } else if (!(gObj.options.allowGrouping && parentsUntil(e.target as Element, 'e-groupdroparea'))) {
                classList(cloneElement, ['e-notallowedcur'], ['e-defaultcur']);
            }
        }
        //gObj.trigger(events.columnDrag, { target: target, draggableType: 'headercell', column: e.column });
    }

    private updateScrollPostion(e: MouseEvent | TouchEvent): void {
        const x: number = getPosition(e).x;
        const cliRect: ClientRect = this.parent.element.getBoundingClientRect();
        const cliRectBaseLeft: number = cliRect.left;
        const cliRectBaseRight: number = cliRect.right;
        const scrollElem: Element = this.parent.getContent();
        if (this.parent.options.frozenName !== 'None') {
            this.updateFrozenScrollPosition(x, cliRect);
        }
        else {
            if (x > cliRectBaseLeft && x < cliRectBaseLeft + 35) {
                this.timer = window.setInterval(
                    () => { this.setScrollLeft(scrollElem, true); }, 50);
            } else if (x < cliRectBaseRight && x > cliRectBaseRight - 35) {
                this.timer = window.setInterval(
                    () => { this.setScrollLeft(scrollElem, false); }, 50);
            }
        }

    }

    private updateFrozenScrollPosition(x: number, cliRect: ClientRect): void {
        const scrollElem: Element = this.parent.getContent().querySelector('.e-movablecontent');
        const mhdrCliRect: ClientRect = this.parent.element.querySelector('.e-movableheader').getBoundingClientRect();
        const left: number = this.parent.options.frozenLeftCount || this.parent.options.actualFrozenColumns;
        const right: number = this.parent.options.frozenRightCount;
        const cliRectBaseRight: number = right ? mhdrCliRect.right : cliRect.right;
        const cliRectBaseLeft: number = left ? mhdrCliRect.left : cliRect.left;
        if (x > cliRectBaseLeft && x < cliRectBaseLeft + 35) {
            this.timer = window.setInterval(() => { this.setScrollLeft(scrollElem, true); }, 50);
        }
        else if (x < cliRectBaseRight && x > cliRectBaseRight - 35) {
            this.timer = window.setInterval(() => { this.setScrollLeft(scrollElem, false); }, 50);
        }
    }

    private setScrollLeft(scrollElem: Element, isLeft: boolean): void {
        const scrollLeft: number = scrollElem.scrollLeft;
        scrollElem.scrollLeft = scrollElem.scrollLeft + (isLeft ? -5 : 5);
        if (scrollLeft !== scrollElem.scrollLeft) {
            this.setDisplay('none');
        }
    }

    private stopTimer(): void {
        window.clearInterval(this.timer);
    }

    private updateArrowPosition(target: Element, isLeft: boolean): void {
        const cliRect: ClientRect = target.getBoundingClientRect();
        const cliRectBase: ClientRect = this.parent.element.getBoundingClientRect();
        if ((isLeft && cliRect.left < cliRectBase.left) || (!isLeft && cliRect.right > cliRectBase.right)) {
            return;
        }
        this.upArrow.style.top = cliRect.top + cliRect.height - cliRectBase.top - 7 + 'px';
        this.downArrow.style.top = cliRect.top - cliRectBase.top - 2 + 'px';
        this.upArrow.style.left = this.downArrow.style.left = (isLeft ? cliRect.left : cliRect.right) - cliRectBase.left - 4 + 'px';
        if (this.parent.options.enableColumnVirtualization) {
            this.upArrow.style.left = this.downArrow.style.left = Number(this.upArrow.style.left.replace('px', '')) + this.parent.getContent().scrollLeft + 'px';
        }
        this.setDisplay('');
    }

    public dragStart(e: { target: Element, column: Column, event: MouseEvent }): void {
        const gObj: SfGrid = this.parent;
        document.body.classList.add('e-prevent-select');
        const target: Element = e.target as Element;
        this.element = target.classList.contains('e-headercell') ? target as HTMLElement :
            parentsUntil(target, 'e-headercell') as HTMLElement;
        if (isNullOrUndefined(this.element)) {
            this.element = (e.event.target as Element).classList.contains('e-headercell') ? e.event.target as HTMLElement : parentsUntil(e.event.target as HTMLElement, 'e-headercell') as HTMLElement;
        }
        this.draggedHeader = this.element.cloneNode(true) as HTMLElement;
        if (!e.column.allowReordering || e.column.fixedColumn) {
            return;
        }
        this.x = getPosition(e.event).x + gObj.getContent().firstElementChild.scrollLeft;
        // gObj.trigger(events.columnDragStart, {
        //     target: target as Element, draggableType: 'headercell', column: e.column
        // });
    }

    public dragStop(e: { target: Element, event: MouseEvent, column: Column, cancel: boolean }): void {
        const gObj: SfGrid = this.parent;
        if (this.parent.options.allowGrouping && e.event.type === 'touchend') {
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            EventHandler.remove(window as any, 'touchmove', this.parent.groupModule.preventTouchOnWindow);
        }
        document.body.classList.remove('e-prevent-select');
        this.setDisplay('none');
        this.stopTimer();
        if (!e.cancel) {
            //gObj.trigger(events.columnDrop, { target: e.target, draggableType: 'headercell', column: e.column });
        }
        removeClass(gObj.element.querySelector('.e-headercontent').querySelectorAll('.e-reorderindicate'), ['e-reorderindicate']);
    }

    private setDisplay(display: string): void {
        if (this.upArrow) {
            this.upArrow.style.display = display;
        }
        if (this.downArrow) {
            this.downArrow.style.display = display;
        }
    }

    private stackedLockedColumn(columnsList: Column[], dropElement: Element): boolean {
        const dropIndex: number = parseInt(dropElement.getAttribute('aria-colindex'), 10) - 1;
        if (!columnsList.some((column: Column) => column.fixedColumn)) {
            return false;
        }
        for (let i: number = 0; i < columnsList.length; i++) {
            const column: Column = columnsList[parseInt(i.toString(), 10)];
            if (!isNullOrUndefined(column.columns) && column.columns.length > 0) {
                // Recursively check nested columns
                const isLockedChild: boolean = this.stackedLockedColumn(column.columns, dropElement);
                if (isLockedChild) {
                    return true; // Return true if any of the child level column is locked
                }
            }
            // Check if the looped index matches dropIndex
            if (i === dropIndex) {
                return column.fixedColumn ? true : false; // Return false if not locked
            }
        }
        return false;
    }

    private getAllStackedheaderParentColumns(headers: Element[]): Column[] {
        const stackedColumns: Column[] = [];
        for (let i: number = 0; i < headers.length; i++) {
            const headerElement: Element = headers[parseInt(i.toString(), 10)];
            if (headerElement.classList.contains('e-hide')) {
                headers.splice(i, 1);
                i--;
            }
            else if (headerElement.closest('thead').firstChild === headerElement.parentElement) {
                const mappingElement: HTMLElement = headerElement.querySelector('[e-mappinguid]');
                const mappingUID: string = !isNullOrUndefined(mappingElement) ? mappingElement.getAttribute('e-mappinguid') : '';
                stackedColumns.push(this.parent.getColumnByUid(mappingUID));
            }
        }
        return stackedColumns;
    }

    public isAnyColumnFixed(column: Column): boolean {
        if (!isNullOrUndefined(column)) {
            // Check if the current column is locked
            if (column.fixedColumn) {
                return true;
            }

            // Check recursively if any of the nested child columns are locked
            if (!isNullOrUndefined(column.columns) && column.columns.length > 0) {
                // Recursively checking each child column
                for (let j: number = 0; j < column.columns.length; j++) {
                    if (this.isAnyColumnFixed(column.columns[parseInt(j.toString(), 10)])) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private setStackedFixedColumns(column: Column, isLocked: boolean): Column {
        const jsonString: string = JSON.stringify(column);
        const stackedColumn: Column = JSON.parse(jsonString);
        const columnsRemoveCount: number = stackedColumn.columns.length;
        for (let j: number = 0; j < column.columns.length; j++) {
            const innerColumn: Column = column.columns[parseInt(j.toString(), 10)];
            if (!innerColumn.columns) {
                if (innerColumn.fixedColumn && isLocked) {
                    stackedColumn.columns.push(innerColumn);
                } else if (!innerColumn.fixedColumn && !isLocked) {
                    stackedColumn.columns.push(innerColumn);
                }
            } else {
                const stackedLockedcolumn: Column = this.setStackedFixedColumns(innerColumn, isLocked);
                if (stackedLockedcolumn.columns.length !== 0) {
                    stackedColumn.columns.push(stackedLockedcolumn);
                }
            }
        }
        if (columnsRemoveCount !== 0) {
            stackedColumn.columns.splice(0, columnsRemoveCount);
        }
        return stackedColumn;
    }

    public processStackedColumns(stackedColumns: Column[]): Column[] {
        const fixedColumns: Column[] = [];
        const unfixedColumns: Column[] = [];

        if (this.parent.checkFixedColumns(stackedColumns)) {
            stackedColumns.forEach((column: Column) => {
                if (column.columns && this.parent.checkFixedColumns(column.columns)) {
                    const fixedColumn: Column = this.setStackedFixedColumns(column, true);
                    const normalColumn: Column = this.setStackedFixedColumns(column, false);

                    if (fixedColumn.columns.length > 0) {
                        fixedColumns.push(fixedColumn);
                    }
                    if (normalColumn.columns.length > 0) {
                        unfixedColumns.push(normalColumn);
                    }
                } else if (column.fixedColumn) {
                    fixedColumns.push(column);
                } else {
                    unfixedColumns.push(column);
                }
            });

            unfixedColumns.sort((a: Column, b: Column) => a.index - b.index);
            const allColumns: Column[] = fixedColumns.concat(unfixedColumns);

            return allColumns; // Processed list of all columns
        }

        return stackedColumns; // Return original if no frozen columns found
    }

    /**
     * For internal use only - Get the module name.
     *
     * @private
     * @returns {string} The name of the module.
     */
    protected getModuleName(): string {
        return 'reorder';
    }
}
