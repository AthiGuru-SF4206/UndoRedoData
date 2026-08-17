import { SfGrid } from './sf-grid-fn';
import { EventHandler, remove, MouseEventArgs, createElement, closest, isNullOrUndefined } from '@syncfusion/ej2-base';
import { parentsUntil, getPosition, addRemoveActiveClasses } from './util';
import { IPosition, Column } from './interfaces';

export class Selection {

    private parent: SfGrid;
    private element: HTMLElement;
    private isCellDrag: boolean;
    private isDragged: boolean;
    private isAutoFillSel: boolean;
    private startDragIndex: number;
    private startCell: Element;
    private endCells: Element;
    private endCell: Element;
    private startAFCell: Element;
    private startIndex: number;
    private startCellIndex: number;
    private endAFCell: Element;
    private startRowIndex: number;
    private endRowIndex: number;
    private startColIndex: number;
    private endColIndex: number;
    private prevStartDIndex: number;
    private prevEndIndex: number;
    private isInitialSelect: boolean;
    private startDragCellIndex: number;
    private endDragIndex: number;
    private endDragCellIndex: number;
    private x: number;
    private y: number;

    constructor(parent: SfGrid) {
        this.parent = parent;
        this.addEventListener();
    }

    private addEventListener(): void {
        EventHandler.add(this.parent.getContent().parentElement, 'mousedown', this.mouseDownHandler, this);
        if (this.parent.options.allowDragSelection) {
            EventHandler.add(this.parent.getContent().parentElement, 'touchstart', this.mouseDownHandler, this);
        }
    }

    public removeEventListener(): void {
        EventHandler.remove(this.parent.getContent().parentElement, 'mousedown', this.mouseDownHandler);
        if (this.parent.options.allowDragSelection) {
            EventHandler.remove(this.parent.getContent().parentElement, 'touchstart', this.mouseDownHandler);
        }
    }

    private mouseDownHandler(e: MouseEventArgs): void {
        const target: Element = e.target as Element;
        const gObj: SfGrid = this.parent;
        let isDrag: boolean;
        const gridElement: Element = parentsUntil(target, 'e-grid');
        if (gObj.options.isRenderedFromGantt && !gObj.options.allowDragSelection) {
            return;
        }
        if (gObj.rowDragAndDropModule.isTargetInEditMode(target)) {
            return;
        }
        if (gridElement && gridElement.id !== gObj.element.id || parentsUntil(target, 'e-headercontent') && !this.parent.options.frozenRows) {
            return;
        }
        if (e.shiftKey || e.ctrlKey) {
            e.preventDefault();
        }
        // Ensure focus on template cell during multi-select (Ctrl/Shift + left mouse button click)
        if (gObj.options.selectionType === 'Multiple' && (e.ctrlKey || e.shiftKey) && e.button === 0) {
            const templateCell: HTMLElement | null = parentsUntil(e.target as HTMLElement, 'e-templatecell') as HTMLElement | null;
            if (templateCell) {
                templateCell.focus();
            }
        }
        if (parentsUntil(target, 'e-rowcell') && !e.shiftKey && !e.ctrlKey) {
            if ((gObj.options.cellSelectionMode.indexOf('Box') > -1 || (gObj.options.allowDragSelection && !gObj.options.enableAutoFill && gObj.options.cellSelectionMode.indexOf('Flow')) > -1) && e.button !== 2 && !this.isRowType() && !this.isSingleSel()) {
                this.isCellDrag = true;
                isDrag = true;
            } else {
                this.isCellDrag = false;
            }
            if ((gObj.options.allowRowDragAndDrop || (gObj.options.allowDragSelection && !gObj.options.enableAutoFill))
                && !gObj.options.isEdit && e.button !== 2) {
                if ((!gObj.options.allowDragSelection && (!this.isRowType() || closest(target, 'td').classList.contains('e-selectionbackground'))) || this.isSingleSel()) {
                    if (gObj.options.allowDragSelection) {
                        document.body.classList.add('e-disableuserselect');
                    }
                    this.isDragged = false;
                    return;
                }
                isDrag = true;
                if (isNullOrUndefined(gridElement.querySelector('.e-griddragarea'))) {
                    this.element = createElement('div', { className: 'e-griddragarea' });
                    this.element.style.left = 0 + 'px';
                    this.element.style.top = 0 + 'px';
                    this.element.style.zIndex = '10';
                    gObj.getContent().appendChild(this.element);
                }
            }
            if (isDrag) {
                this.isAutoFillSel = false;
                this.enableDrag(e, true);
            }
        }
        this.updateStartEndCells();
        if (target.classList.contains('e-autofill') || target.classList.contains('e-xlsel')) {
            this.isCellDrag = true;
            this.isAutoFillSel = true;
            this.enableDrag(e);
            document.body.style.cursor = 'crosshair';
        }
    }

    private mouseUpHandler(e: MouseEventArgs): void {
        document.body.classList.remove('e-disableuserselect');
        if (!isNullOrUndefined(this.element) && !isNullOrUndefined(this.element.parentNode)) {
            remove(this.element);
        }
        const targetGrid: Element = parentsUntil(e.target as Element, 'e-grid');
        const targetGridId: string | null = targetGrid ? targetGrid.id : null;
        if (!this.isCellDrag && (!isNullOrUndefined(this.prevStartDIndex) && !isNullOrUndefined(this.prevEndIndex))
            && !isNaN(this.prevStartDIndex) && !isNaN(this.prevEndIndex)) {
            const targetElement: Element = closest(e.target as Element, 'td');
            const endDragCellIndex: number = targetElement ? parseInt(targetElement.getAttribute('aria-colindex'), 10) - 1 : NaN;
            if (!isNaN(this.startDragCellIndex) && !isNaN(endDragCellIndex)
                && !isNullOrUndefined(this.startDragCellIndex) && !isNullOrUndefined(endDragCellIndex)) {
                this.parent.dotNetRef.invokeMethodAsync('DragSelection', this.prevStartDIndex, this.prevEndIndex, false, targetGridId, this.startDragCellIndex, endDragCellIndex);
            }
            else {
                this.parent.dotNetRef.invokeMethodAsync('DragSelection', this.prevStartDIndex, this.prevEndIndex, false, targetGridId, 0, 0);
            }
        }
        else if ((this.parent.options.allowDragSelection && !this.parent.options.enableAutoFill) && this.isDragged && !this.isAutoFillSel
            && (!isNullOrUndefined(this.startDragIndex) && !isNullOrUndefined(this.endDragIndex)
            && !isNullOrUndefined(this.endDragCellIndex))) {
            this.isDragged = false;
            this.parent.dotNetRef.invokeMethodAsync('DragCellSelection', this.startDragIndex, this.startDragCellIndex, this.endDragIndex, this.endDragCellIndex, false, targetGridId);
        }
        this.isDragged = false;
        document.body.style.cursor = 'default';
        this.prevStartDIndex = undefined;
        this.prevEndIndex = undefined;
        this.startDragIndex = undefined;
        this.startDragCellIndex = undefined;
        this.endDragIndex = undefined;
        this.endDragCellIndex = undefined;
        if (this.parent.options.editMode === 'Batch' && this.parent.options.enableAutoFill) {
            if (!isNullOrUndefined(this.endRowIndex) && !isNullOrUndefined(this.endColIndex)
                && !this.isAutoFillSel && this.isInitialSelect) {
                this.parent.dotNetRef.invokeMethodAsync('ClearSelection');
                const updateAFPos: object = this.updateAutofillPosition(this.endColIndex, this.endRowIndex, true);
                this.parent.dotNetRef.invokeMethodAsync('UpdateAutofillPositions', updateAFPos, 'UpdateAutofillBox');
                this.assignCells();
                this.selectCellByRow();
                this.isInitialSelect = false;
            }
            if (this.isAutoFillSel) {
                // eslint-disable-next-line @typescript-eslint/no-this-alias
                const _this: Selection = this;
                this.assignCells();
                setTimeout(() => {
                    _this.selectCellByRow();
                }, 10);
                this.expandAFBorder(e, true);
                const updateAFBor: object = this.createBorder(this.startRowIndex, this.startColIndex, this.endRowIndex,
                                                              this.endColIndex, true);
                this.parent.dotNetRef.invokeMethodAsync('UpdateAutofillPositions', updateAFBor, 'UpdateAutofillBorder');
                const updateAFPos: object = this.updateAutofillPosition(this.endColIndex, this.endRowIndex, true);
                this.parent.dotNetRef.invokeMethodAsync('UpdateAutofillPositions', updateAFPos, 'UpdateAutofillBox');
            }
        }
        EventHandler.remove(this.parent.getContent(), 'mousemove', this.mouseMoveHandler);
        if (this.parent.options.allowDragSelection) {
            EventHandler.remove(this.parent.getContent(), 'touchmove', this.mouseMoveHandler);
        }
        if (this.parent.options.frozenRows) {
            EventHandler.remove(this.parent.getHeaderContent(), 'mousemove', this.mouseMoveHandler);
            if (this.parent.options.allowDragSelection) {
                EventHandler.remove(this.parent.getHeaderContent(), 'touchmove', this.mouseMoveHandler);
            }
        }
        EventHandler.remove(document.body, 'mouseup', this.mouseUpHandler);
        if (this.parent.options.allowDragSelection) {
            EventHandler.remove(document.body, 'touchend', this.mouseUpHandler);
        }
    }

    private enableDrag(e: MouseEventArgs, isUpdate?: boolean): void {
        const gObj: SfGrid = this.parent;
        if (isUpdate) {
            const tr: Element = closest(e.target as Element, 'tr');
            this.startDragIndex = parseInt(tr.getAttribute('aria-rowindex'), 10) - 1;
            this.startDragCellIndex = parseInt(parentsUntil(e.target as Element, 'e-rowcell').getAttribute('aria-colindex'), 10) - 1;
        }
        document.body.classList.add('e-disableuserselect');
        const gBRect: ClientRect = gObj.element.getBoundingClientRect();
        const postion: IPosition = getPosition(e);
        this.x = postion.x - gBRect.left;
        this.y = postion.y - gBRect.top;
        EventHandler.add(gObj.getContent(), 'mousemove', this.mouseMoveHandler, this);
        if (this.parent.options.allowDragSelection) {
            EventHandler.add(gObj.getContent(), 'touchmove', this.mouseMoveHandler, this);
        }
        if (this.parent.options.frozenRows) {
            EventHandler.add(gObj.getHeaderContent(), 'mousemove', this.mouseMoveHandler, this);
            if (this.parent.options.allowDragSelection) {
                EventHandler.add(gObj.getHeaderContent(), 'touchmove', this.mouseMoveHandler, this);
            }
        }
        EventHandler.add(document.body, 'mouseup', this.mouseUpHandler, this);
        if (this.parent.options.allowDragSelection) {
            EventHandler.add(document.body, 'touchend', this.mouseUpHandler, this);
        }
        if (!isNullOrUndefined(this.startDragIndex) && !isNullOrUndefined(this.startDragCellIndex)
            && !isNaN(this.startDragIndex) && !isNaN(this.startDragCellIndex)) {
            this.parent.dotNetRef.invokeMethodAsync('DragSelectionStarted', this.startDragIndex, this.startDragCellIndex);
        }
    }

    private mouseMoveHandler(e: MouseEventArgs | TouchEvent): void {
        if (!this.parent.options.allowDragSelection && e.type !== 'touchmove') {
            e.preventDefault();
        }
        const gBRect: ClientRect = this.parent.element.getBoundingClientRect();
        let x1: number = this.x;
        let y1: number = this.y;
        const position: IPosition = getPosition(e);
        let x2: number = position.x - gBRect.left;
        let y2: number = position.y - gBRect.top;
        const xPos: number = e.type === 'touchmove' ? (e as TouchEvent).touches[0].pageX : (e as MouseEventArgs).pageX;
        const yPos: number = e.type === 'touchmove' ? (e as TouchEvent).touches[0].pageY : (e as MouseEventArgs).pageY;
        let eleLocation: number = yPos + 2;
        let tmp: number;
        let target: Element;
        if (!isNullOrUndefined((e as TouchEvent).touches)
            && !isNullOrUndefined(document.elementFromPoint((e as TouchEvent).touches[0].clientX,
                                                            (e as TouchEvent).touches[0].clientY))) {
            target = closest(document.elementFromPoint((e as TouchEvent).touches[0].clientX, (e as TouchEvent).touches[0].clientY) as Element, 'tr');
        } else {
            target = closest(e.target as Element, 'tr');
        }
        this.isDragged = true;
        if (!this.isCellDrag || ((this.parent.options.allowDragSelection && !this.parent.options.enableAutoFill) && this.isCellDrag)) {
            if (!target) {
                target = closest(document.elementFromPoint(this.parent.element.offsetLeft + 2, !isNullOrUndefined((e as TouchEvent).touches) ? (e as TouchEvent).touches[0].clientY : (e as MouseEventArgs).clientY), 'tr');
            }
            if (x1 > x2) {
                tmp = x2;
                x2 = x1;
                x1 = tmp;
            }
            if (y1 > y2) {
                tmp = y2;
                y2 = y1;
                y1 = tmp;
                eleLocation = yPos - 2;
            }
            const classList: string[] = ['.e-gridheader', '.e-groupdroparea', '.e-toolbar'];
            let siblingHeight: number = 0;
            for (let i: number = 0; i < classList.length; i++) {
                const sibling: HTMLElement = this.parent.element.querySelector(classList[parseInt(i.toString(), 10)]);
                if (sibling) {
                    siblingHeight += sibling.offsetHeight;
                }
            }
            const topHeight: HTMLElement = this.parent.element.querySelector('.e-yscroll');
            let scrollTopHeight: number = 0;
            if (topHeight) {
                scrollTopHeight += topHeight.scrollTop;
            }
            const Content: HTMLElement = this.parent.element.querySelector('.e-content') as HTMLElement;
            this.element.style.left = x1 + Content.scrollLeft + 2 + 'px';
            this.element.style.top = y1 - siblingHeight + scrollTopHeight + 'px';
            this.element.style.width = x2 - x1 + 'px';
            this.element.style.height = y2 - y1 + 'px';
            this.element.style.pointerEvents = 'none';
        }
        if (target && !e.ctrlKey && !e.shiftKey) {
            const rowIndex: number = parseInt(target.getAttribute('aria-rowindex'), 10) - 1;
            if (!this.isCellDrag && (isNullOrUndefined(this.prevStartDIndex) ||
                this.prevStartDIndex !== this.startDragIndex || this.prevEndIndex !== rowIndex)) {
                //Below calculation is to perform ClearSelection in server side
                let clearIndex: number = -1;
                let isInvokedFirst: boolean = false;
                const selectedIndexes: number[] = this.parent.getSelectedRowIndexes(this.parent.options.enableVirtualization);
                if (isNullOrUndefined(this.prevStartDIndex)) {
                    clearIndex = -1;
                    isInvokedFirst = true;
                } else if (rowIndex >= this.prevStartDIndex && selectedIndexes.indexOf(rowIndex) >= 0) {
                    clearIndex = this.prevEndIndex;
                } else if (this.prevStartDIndex > rowIndex && selectedIndexes.indexOf(this.startDragIndex) >= 0) {
                    clearIndex = this.prevEndIndex;
                }
                this.prevStartDIndex = this.startDragIndex;
                this.prevEndIndex = rowIndex;
                if (isInvokedFirst && !isNaN(rowIndex)) {
                    this.parent.dotNetRef.invokeMethodAsync('DragSelection', this.startDragIndex, rowIndex, true, null, 0, 0);
                } else {
                    this.performDragSelection(this.startDragIndex, rowIndex, clearIndex);
                }
            } else if ((this.parent.options.allowDragSelection && !this.parent.options.enableAutoFill) && this.isCellDrag) {
                let target: Element;
                let isInvokedFirst: boolean = false;
                if (!isNullOrUndefined((e as TouchEvent).touches)
                    && !isNullOrUndefined(document.elementFromPoint((e as TouchEvent).touches[0].clientX,
                                                                    (e as TouchEvent).touches[0].clientY))) {
                    target = closest(document.elementFromPoint((e as TouchEvent).touches[0].clientX, (e as TouchEvent).touches[0].clientY) as Element, 'td');
                } else {
                    target = closest(e.target as Element, 'td');
                }
                if (!target) {
                    target = document.elementFromPoint(xPos, eleLocation);
                }
                if (target) {
                    if (isNullOrUndefined(this.prevStartDIndex)) {
                        isInvokedFirst = true;
                    }
                    this.prevStartDIndex = this.startDragIndex;
                    this.endDragIndex = rowIndex;
                    this.endDragCellIndex = parseInt(target.getAttribute('aria-colindex'), 10) - 1;
                    if (!this.isAutoFillSel) {
                        if (isInvokedFirst && !isNaN(rowIndex) && !isNaN(parseInt(target.getAttribute('aria-colindex'), 10) - 1)) {
                            this.parent.dotNetRef.invokeMethodAsync('DragCellSelection', this.startDragIndex, this.startDragCellIndex, rowIndex, (parseInt(target.getAttribute('aria-colindex'), 10) - 1), true, null);
                        } else {
                            this.performDragCellSelection(this.startDragIndex, this.startDragCellIndex, rowIndex, (parseInt(target.getAttribute('aria-colindex'), 10) - 1));
                        }
                    }
                }
            }
            else if (this.parent.options.editMode === 'Batch' && this.parent.options.enableAutoFill) {
                if (this.startCell) {
                    const td: Element = parentsUntil(e.target as HTMLElement, 'e-rowcell');
                    if (td && !td.classList.contains('e-editedbatchcell')) {
                        this.startAFCell = this.startCell;
                        this.endAFCell = td;
                        this.endCell = td;
                        if (this.isAutoFillSel) {
                            this.expandAFBorder((e as MouseEventArgs), false);
                        }
                        else {
                            this.assignCells();
                            const updateAFBor: object = this.createBorder(this.startRowIndex, this.startColIndex,
                                                                          this.endRowIndex, this.endColIndex, true);
                            this.parent.dotNetRef.invokeMethodAsync('UpdateAutofillPositions', updateAFBor, 'UpdateAutofillBorder');
                            this.isInitialSelect = true;
                        }
                    }
                }
            }
        }
    }


    private performDragCellSelection(startIndex: number, startCellIndex: number, rowIndex: number, endCellIndex: number): void {
        let sIndex: number = startIndex;
        let eIndex: number = rowIndex;
        if (startIndex > rowIndex) {
            sIndex = rowIndex;
            eIndex = startIndex;
        }
        this.selectCellsByRange(sIndex, eIndex, startCellIndex, endCellIndex);
    }

    private selectCellsByRange(sIndex: number, eIndex: number, startCellIndex: number, endCellIndex: number): void {

        const HeaderRows: NodeListOf<Element> = this.parent.getHeaderContent().querySelectorAll('tr.e-row[data-uid]');
        const ContentRows: NodeListOf<Element> = this.parent.getContent().querySelectorAll('tr.e-row[data-uid]');
        const rows: Element[] = Array.from(HeaderRows).concat(Array.from(ContentRows));
        for (let i: number = 0; i < rows.length; i++) {
            const cells: Element[] = [].slice.call(rows[parseInt(i.toString(), 10)].querySelectorAll('.e-rowcell'));
            rows[parseInt(i.toString(), 10)].removeAttribute('aria-selected');
            addRemoveActiveClasses(cells, false, ...['e-aria-selected', 'e-active']);
        }

        let min: number;
        let max: number;
        const cells: Element[] = [];
        if (sIndex > eIndex) {
            const temp: number = sIndex;
            sIndex = eIndex;
            eIndex = temp;
        }
        if (startCellIndex > endCellIndex) {
            const Celltemp: number = startCellIndex;
            startCellIndex = endCellIndex;
            endCellIndex = Celltemp;
        }
        for (let i: number = sIndex; i <= eIndex; i++) {
            if (this.parent.options.cellSelectionMode.indexOf('Box') < 0) {
                min = i === sIndex ? startCellIndex : 0;
                max = i === eIndex ? endCellIndex : this.getLastColIndex(i);
            } else {
                min = startCellIndex;
                max = endCellIndex;
            }
            for (let j: number = min < max ? min : max, len: number = min > max ? min : max; j <= len; j++) {
                cells.push(this.getCellIndex(i, j));
            }
            addRemoveActiveClasses(cells, true, ...['e-aria-selected', 'e-active']);
        }
    }

    private getCellIndex(rowIndex: number, cellIndex: number): Element {
        const HeaderRows: NodeListOf<Element> = this.parent.getHeaderContent().querySelectorAll('tr.e-row[data-uid]');
        const ContentRows: NodeListOf<Element> = this.parent.getContent().querySelectorAll('tr.e-row[data-uid]');
        const rows: Element[] = Array.from(HeaderRows).concat(Array.from(ContentRows));
        return rows[parseInt(rowIndex.toString(), 10)] && rows[parseInt(rowIndex.toString(), 10)].querySelectorAll('.e-rowcell')[parseInt(cellIndex.toString(), 10)];
    }

    private getLastColIndex(rowIndex: number): number {
        const cells: NodeListOf<Element> = this.parent.getDataRows()[parseInt(rowIndex.toString(), 10)].querySelectorAll('td.e-rowcell');
        return parseInt(cells[cells.length - 1].getAttribute('aria-colindex'), 10) - 1;
    }
    /**
     * Update the position of the autofill handle.
     *
     * @param {number} cellindex - The column index of the cell.
     * @param {number} index - The row index of the cell.
     * @param {boolean} [newSelect=false] - Whether the selection is new or not.
     * @returns {object} - The updated position information of the autofill handle.
     * @hidden
     */
    public updateAutofillPosition(cellindex: number, index: number, newSelect: boolean = false): object {
        const row: Element = this.parent.getRowByIndex(index);
        let cell: HTMLElement = row.querySelector('[aria-colindex="' + (cellindex + 1) + '"]');
        const selectedCells: Element[] = [].slice.call(this.parent.element.querySelectorAll('.e-cellselectionbackground'));
        let autoFillBoxLeft: string = '';
        let autoFillBoxRight: string = '';
        let autoFillBoxTop: string = '';
        if (selectedCells && !newSelect) {
            cell = selectedCells[selectedCells.length - 1] as HTMLElement;
        }
        if (cell && cell.offsetParent) {
            const clientRect: ClientRect = cell.getBoundingClientRect();
            const parentOff: ClientRect = cell.offsetParent.getBoundingClientRect();
            const colWidth: number = this.isLastCell(cell) ? 4 : 0;
            const rowHeight: number = this.isLastRow(cell) ? 3 : 0;
            if (!this.parent.options.enableRtl) {
                autoFillBoxLeft = clientRect.left - parentOff.left + clientRect.width - 4 - colWidth + 'px';
            }
            else {
                autoFillBoxRight = parentOff.right - clientRect.right + clientRect.width - 4 - colWidth + 'px';
            }
            autoFillBoxTop = clientRect.top - parentOff.top + clientRect.height - 5 - rowHeight + 'px';
        }
        return {
            Left: autoFillBoxLeft,
            Right: autoFillBoxRight,
            Top: autoFillBoxTop
        };
    }
    /**
     * Creates a border around the specified cell range.
     *
     * @param {number} startRowIndex - The starting row index of the cell range.
     * @param {number} startColIndex - The starting column index of the cell range.
     * @param {number} [endRowIndex=null] - The ending row index of the cell range.
     * @param {number} [endColIndex=null] - The ending column index of the cell range.
     * @param {boolean} [newSelect=false] - Whether the selection is new or not.
     * @returns {object} - The updated border information.
     * @hidden
     */
    public createBorder(startRowIndex: number, startColIndex: number, endRowIndex: number = null, endColIndex: number = null,
                        newSelect: boolean = false): object {
        const selectedCells: Element[] = [].slice.call(this.parent.element.querySelectorAll('.e-cellselectionbackground'));
        const rowstart: Element = this.parent.getRowByIndex(startRowIndex);
        const cellStart: Element = rowstart.querySelector('[aria-colindex="' + (startColIndex + 1) + '"]');
        let cellsStart: HTMLElement[] = [].slice.call(cellStart.parentElement.querySelectorAll('[aria-colindex="' + (startColIndex + 1) + '"]'));
        let rowEnd: Element;
        let cellEnd: Element;
        let cellsEnd: HTMLElement[];
        let autoFillBorderRight: string = '';
        let autoFillBorderLeft: string = '';
        let autoFillBordersWidth: string = '';
        let autoFillBorderWidth: string = '';
        let autoFillBorderHeight: string = '';
        let autoFillBorderTop: string = '';
        if (endRowIndex != null && endColIndex != null) {
            rowEnd = this.parent.getRowByIndex(endRowIndex);
            cellEnd = rowEnd.querySelector('[aria-colindex="' + (endColIndex + 1) + '"]');
            cellsEnd = [].slice.call(cellEnd.parentElement.querySelectorAll('[aria-colindex="' + (endColIndex + 1) + '"]'));
        }
        else {
            rowEnd = rowstart;
            cellEnd = cellStart;
            cellsEnd = cellsStart;
        }
        if (selectedCells && !newSelect) {
            cellsStart = [].slice.call(selectedCells[0].parentElement.querySelectorAll('[aria-colindex="' +
                ((selectedCells[0] as HTMLTableCellElement).cellIndex - this.parent.getIndentCount() + 1) + '"]'));
            cellsEnd = [].slice.call(selectedCells[selectedCells.length - 1].parentElement.querySelectorAll('[aria-colindex="' +
                ((selectedCells[selectedCells.length - 1] as HTMLTableCellElement).cellIndex - this.parent.getIndentCount() + 1) + '"]'));
        }
        if (!this.startCell) {
            this.startCell = cellsStart[0];
        }

        this.endCells = cellsEnd[0];
        const start: HTMLElement = cellsStart[0] as HTMLElement;
        const end: HTMLElement = cellsEnd[0] as HTMLElement;
        const stOff: ClientRect = start.getBoundingClientRect();
        const endOff: ClientRect = end.getBoundingClientRect();
        const parentOff: ClientRect = start.offsetParent.getBoundingClientRect();
        const rowHeight: number = this.isLastRow(end) ? 2 : 0;
        const topOffSet: number = this.parent.options.frozenRows && this.isFirstRow(start) ? 1.5 : 0;
        const leftOffset: number = this.parent.options.frozenColumns && this.isFirstCell(start) ? 1 : 0;
        if (this.parent.options.enableRtl) {
            autoFillBorderRight = parentOff.right - stOff.right - leftOffset + 'px';
            autoFillBorderWidth = stOff.right - endOff.left + leftOffset + 1 + 'px';
        } else {
            autoFillBorderLeft = stOff.left - parentOff.left - leftOffset + 'px';
            autoFillBorderWidth = endOff.right - stOff.left + leftOffset + 1 + 'px';
        }
        autoFillBorderTop = stOff.top - parentOff.top - topOffSet + 'px';
        autoFillBorderHeight = endOff.top - stOff.top > 0 ?
            (endOff.top - parentOff.top + endOff.height + 1) - (stOff.top - parentOff.top) - rowHeight + topOffSet + 'px' :
            endOff.height + topOffSet - rowHeight + 1 + 'px';
        autoFillBordersWidth = '2px';
        return {
            Right: autoFillBorderRight,
            Width: autoFillBorderWidth,
            BorderWidth: autoFillBordersWidth,
            Left: autoFillBorderLeft,
            Height: autoFillBorderHeight,
            Top: autoFillBorderTop
        };
    }
    private expandAFBorder(e: MouseEvent, isApply: boolean): void {
        const selectedCells: Element[] = [].slice.call(this.parent.element.querySelectorAll('.e-cellselectionbackground'));
        const startrowIdx: number = parseInt(parentsUntil(this.startCell, 'e-row').getAttribute('aria-rowindex'), 10) - 1;
        const startCellIdx: number = parseInt(this.startCell.getAttribute('aria-colindex'), 10) - 1;
        let endrowIdx: number = parseInt(parentsUntil(this.endCell, 'e-row').getAttribute('aria-rowindex'), 10) - 1;
        let endCellIdx: number = parseInt(this.endCell.getAttribute('aria-colindex'), 10) - 1;
        const rowLen: number = (parseInt(parentsUntil(selectedCells[selectedCells.length - 1], 'e-row').getAttribute('aria-rowindex'), 10) - 1) - (parseInt(parentsUntil(selectedCells[0], 'e-row').getAttribute('aria-rowindex'), 10) - 1);
        const rowIdx: number = parseInt(parentsUntil(selectedCells[0], 'e-row').getAttribute('aria-rowindex'), 10) - 1;
        const row: HTMLTableRowElement = <HTMLTableRowElement>(this.parent.getRowByIndex(rowIdx));
        let colLen: number = 0;
        for (let i: number = 0, cellLen: number = row.cells.length; i < cellLen; i++) {
            if (row.cells[parseInt(i.toString(), 10)].classList.contains('e-cellselectionbackground')) {
                colLen++;
            }
        }
        colLen = colLen - 1;
        colLen = colLen >= 0 ? colLen : 0;
        switch (true) {
        case !isApply && this.endAFCell.classList.contains('e-cellselectionbackground') &&
            !!parentsUntil(e.target as Element, 'e-rowcell'):
            this.startAFCell = this.parent.getCellFromIndex(startrowIdx, startCellIdx);
            this.endAFCell = this.parent.getCellFromIndex(startrowIdx + rowLen, startCellIdx + colLen);
            this.drawAFBorders();
            break;
        case startCellIdx + colLen < endCellIdx &&
            endCellIdx - startCellIdx - colLen + 1 > endrowIdx - startrowIdx - rowLen
            && endCellIdx - startCellIdx - colLen + 1 > startrowIdx - endrowIdx:
            this.endAFCell = this.parent.getCellFromIndex(startrowIdx + rowLen, endCellIdx);
            endrowIdx = parseInt(parentsUntil(this.endAFCell, 'e-row').getAttribute('aria-rowindex'), 10) - 1;
            endCellIdx = parseInt(this.endAFCell.getAttribute('aria-colindex'), 10) - 1;
            if (!isApply) {
                this.drawAFBorders();
            }
            else {
                const cellIdx: number = parseInt(this.endCells.getAttribute('aria-colindex'), 10) - 1;
                for (let i: number = startrowIdx; i <= endrowIdx; i++) {
                    const cells: HTMLElement[] = this.getAutoFillCells(i);
                    let c: number = 0;
                    for (let j: number = cellIdx + 1; j <= endCellIdx; j++) {
                        if (c > colLen) {
                            c = 0;
                        }
                        this.updateValue(i, j, cells[parseInt(c.toString(), 10)] as HTMLTableCellElement);
                        c++;
                    }
                }
            }
            break;
        case startCellIdx > endCellIdx &&
            startCellIdx - endCellIdx + 1 > endrowIdx - startrowIdx - rowLen &&
            startCellIdx - endCellIdx + 1 > startrowIdx - endrowIdx:
            this.startAFCell = this.parent.getCellFromIndex(startrowIdx, endCellIdx);
            this.endAFCell = this.endCells;
            if (!isApply) {
                this.drawAFBorders();
            }
            else {
                for (let i: number = startrowIdx; i <= startrowIdx + rowLen; i++) {
                    const cells: HTMLElement[] = this.getAutoFillCells(i);
                    cells.reverse();
                    let c: number = 0;
                    for (let j: number = this.startCellIndex - 1; j >= endCellIdx; j--) {
                        if (c > colLen) {
                            c = 0;
                        }
                        this.updateValue(i, j, cells[parseInt(c.toString(), 10)] as HTMLTableCellElement);
                        c++;
                    }
                }
            }
            break;
        case startrowIdx > endrowIdx:
            this.startAFCell = this.parent.getCellFromIndex(endrowIdx, startCellIdx);
            this.endAFCell = this.endCells;
            if (!isApply) {
                this.drawAFBorders();
            }
            else {
                const trIdx: number = parseInt(this.endCells.parentElement.getAttribute('aria-rowindex'), 10) - 1;
                let r: number = trIdx;
                for (let i: number = startrowIdx - 1; i >= endrowIdx; i--) {
                    if (r === this.startIndex - 1) {
                        r = trIdx;
                    }
                    const cells: HTMLElement[] = this.getAutoFillCells(r);
                    let c: number = 0;
                    r--;
                    for (let j: number = this.startCellIndex; j <= this.startCellIndex + colLen; j++) {
                        this.updateValue(i, j, cells[parseInt(c.toString(), 10)] as HTMLTableCellElement);
                        c++;
                    }
                }
            }
            break;
        default:
            this.endAFCell = this.parent.getCellFromIndex(endrowIdx, startCellIdx + colLen);
            if (!isApply) {
                this.drawAFBorders();
            }
            else {
                const trIdx: number = parseInt(this.endCells.parentElement.getAttribute('aria-rowindex'), 10) - 1;
                let r: number = this.startIndex;
                for (let i: number = trIdx + 1; i <= endrowIdx; i++) {
                    if (r === trIdx + 1) {
                        r = this.startIndex;
                    }
                    const cells: HTMLElement[] = this.getAutoFillCells(r);
                    r++;
                    let c: number = 0;
                    for (let m: number = this.startCellIndex; m <= this.startCellIndex + colLen; m++) {
                        this.updateValue(i, m, cells[parseInt(c.toString(), 10)] as HTMLTableCellElement);
                        c++;
                    }
                }
            }
            break;
        }
    }
    private drawAFBorders(): void {
        if (!this.startCell) {
            return;
        }
        const stOff: ClientRect = this.startAFCell.getBoundingClientRect();
        const endOff: ClientRect = this.endAFCell.getBoundingClientRect();
        const top: number = endOff.top - stOff.top > 0 ? 1 : 0;
        const firstCellTop: number = endOff.top - stOff.top >= 0 && (parentsUntil(this.startAFCell, 'e-movablecontent') ||
            parentsUntil(this.startAFCell, 'e-frozencontent')) && this.isFirstRow(this.startAFCell) ? 1.5 : 0;
        const firstCellLeft: number = (parentsUntil(this.startAFCell, 'e-movablecontent') ||
            parentsUntil(this.startAFCell, 'e-movableheader')) && this.isFirstCell(this.startAFCell) ? 1 : 0;
        const rowHeight: number = this.isLastRow(this.endAFCell) && (parentsUntil(this.endAFCell, 'e-movablecontent') ||
            parentsUntil(this.endAFCell, 'e-frozencontent')) ? 2 : 1;
        const parentOff: ClientRect = (this.startAFCell as HTMLElement).offsetParent.getBoundingClientRect();
        const parentRect: ClientRect = this.parent.element.getBoundingClientRect();
        const sTop: number = (this.startAFCell as HTMLElement).offsetParent.parentElement.scrollTop;
        const sLeft: number = (this.startAFCell as HTMLElement).offsetParent.parentElement.scrollLeft;

        let scrollTop: number = sTop - (this.startAFCell as HTMLElement).offsetTop;
        let scrollLeft: number = sLeft - (this.startAFCell as HTMLElement).offsetLeft;
        scrollTop = scrollTop > 0 ? Math.floor(scrollTop) - 1 : 0;
        scrollLeft = scrollLeft > 0 ? scrollLeft : 0;
        const left: number = stOff.left - parentRect.left;

        let bdrAFLeftLeft: string = '';
        let bdrAFLeftHeight: string = '';
        let bdrAFLeftTop: string = '';
        let bdrAFLeftRight: string = '';
        let bdrAFRightLeft: string = '';
        let bdrAFRightHeight: string = '';
        let bdrAFRightRight: string = '';
        let bdrAFRightTop: string = '';
        let bdrAFTopLeft: string = '';
        let bdrAFTopTop: string = '';
        let bdrAFTopWidth: string = '';
        let bdrAFBottomLeft: string = '';
        let bdrAFBottomTop: string = '';
        let bdrAFBottomWidth: string = '';

        if (!this.parent.options.enableRtl) {
            bdrAFLeftLeft = left - firstCellLeft + scrollLeft - 1 + sLeft + 'px';
            bdrAFRightLeft = endOff.left - parentRect.left - 2 + endOff.width + sLeft + 'px';
            bdrAFTopLeft = left + scrollLeft - 0.5 + sLeft + 'px';
            bdrAFTopWidth = parseInt(bdrAFRightLeft, 10) - parseInt(bdrAFLeftLeft, 10)
                - firstCellLeft + 1 + 'px';
        }
        else {
            const scrolloffSet: number = (parentsUntil(this.startAFCell, 'e-movablecontent') ||
                parentsUntil(this.startAFCell, 'e-movableheader')) ? stOff.right -
                (this.startAFCell as HTMLElement).offsetParent.parentElement.getBoundingClientRect().width -
            parentRect.left : 0;
            bdrAFLeftRight = parentRect.right - endOff.right - 2 + endOff.width + 'px';
            bdrAFRightRight = parentRect.right - stOff.right - firstCellLeft + scrolloffSet - 1 + 'px';
            bdrAFTopLeft = endOff.left - parentRect.left - 0.5 + 'px';
            bdrAFTopWidth = parseInt(bdrAFLeftRight, 10) - parseInt(bdrAFRightRight, 10)
                - firstCellLeft + 1 + 'px';
        }
        bdrAFLeftTop = stOff.top - parentOff.top - firstCellTop + 'px';
        bdrAFLeftHeight = endOff.top - stOff.top > 0 ?
            (endOff.top - parentOff.top + endOff.height + 1) - (stOff.top - parentOff.top) + firstCellTop - rowHeight - scrollTop + 'px' :
            endOff.height + firstCellTop - rowHeight - scrollTop + 'px';
        bdrAFRightTop = bdrAFLeftTop;
        bdrAFRightHeight = parseInt(bdrAFLeftHeight, 10) + 'px';
        bdrAFTopTop = bdrAFRightTop;
        bdrAFBottomLeft = bdrAFTopLeft;
        bdrAFBottomTop = parseFloat(bdrAFLeftTop) + parseFloat(bdrAFLeftHeight) - top - 1 + 'px';
        bdrAFBottomWidth = bdrAFTopWidth;
        const positionAF: object = {
            BorderLeftAutofillLeft: bdrAFLeftLeft,
            BorderLeftAutofillTop: bdrAFLeftTop,
            BorderLeftAutofillHeight: bdrAFLeftHeight,
            BorderLeftAutofillRight: bdrAFLeftRight,
            BorderRightAutofillLeft: bdrAFRightLeft,
            BorderRightAutofillHeight: bdrAFRightHeight,
            BorderRightAutofillRight: bdrAFRightRight,
            BorderRightAutofillTop: bdrAFRightTop,
            BorderTopAutofillLeft: bdrAFTopLeft,
            BorderTopAutofillTop: bdrAFTopTop,
            BorderTopAutofillWidth: bdrAFTopWidth,
            BorderBottomAutofillLeft: bdrAFBottomLeft,
            BorderBottomAutofillTop: bdrAFBottomTop,
            BorderBottomAutofillWidth: bdrAFBottomWidth
        };

        this.parent.dotNetRef.invokeMethodAsync('UpdateAutofillPositions', positionAF, 'UpdateAutofillPosition');
    }
    private updateValue(rowIndex: number, colIndex: number, cell: HTMLTableCellElement): void {
        const col: Column = this.parent.getColumnByIndex(colIndex);
        const valueIndex: number = parseInt(parentsUntil(cell, 'e-row').getAttribute('aria-rowindex'), 10) - 1;
        const column: Column = this.parent.getColumnByIndex(cell.cellIndex - this.parent.getIndentCount());
        const value: string = cell.innerText;
        this.parent.dotNetRef.invokeMethodAsync('UpdateAutofillCell', rowIndex, col.field, column.field, valueIndex, value);
    }
    private getAutoFillCells(rowIndex: number): HTMLElement[] {
        const cells: HTMLElement[] = [].slice.call(this.parent.getDataRows()[parseInt(rowIndex.toString(), 10)].querySelectorAll('.e-cellselectionbackground'));
        return cells;
    }
    private updateStartEndCells(): void {
        const cells: Element[] = [].slice.call(this.parent.element.querySelectorAll('.e-cellselectionbackground'));
        this.startCell = cells[0];
        this.endCell = cells[cells.length - 1];
        if (this.startCell) {
            this.startIndex = parseInt(this.startCell.parentElement.getAttribute('aria-rowindex'), 10) - 1;
            this.startCellIndex = parseInt(parentsUntil(this.startCell, 'e-rowcell').getAttribute('aria-colindex'), 10) - 1;
        }
    }
    private assignCells(): void {
        this.startRowIndex = parseInt(this.startAFCell.parentElement.getAttribute('aria-rowindex'), 10) - 1;
        this.endRowIndex = parseInt(this.endAFCell.parentElement.getAttribute('aria-rowindex'), 10) - 1;
        this.startColIndex = parseInt(this.startAFCell.getAttribute('aria-colindex'), 10) - 1;
        this.endColIndex = parseInt(this.endAFCell.getAttribute('aria-colindex'), 10) - 1;
        if (this.startRowIndex > this.endRowIndex) {
            this.startRowIndex = this.endRowIndex;
            this.endRowIndex = parseInt(this.startAFCell.parentElement.getAttribute('aria-rowindex'), 10) - 1;
        }
        if (this.endColIndex < this.startColIndex) {
            this.startColIndex = this.endColIndex;
            this.endColIndex = parseInt(this.startAFCell.getAttribute('aria-colindex'), 10) - 1;
        }
    }
    private selectCellByRow(): void {
        for (let i: number = this.startRowIndex; i <= this.endRowIndex; i++) {
            for (let j: number = this.startColIndex; j <= this.endColIndex; j++) {
                this.parent.dotNetRef.invokeMethodAsync('SelectCellByRow', i, j);
            }
        }
    }
    private isLastCell(cell: Element): boolean {
        const LastCell: Element[] = [].slice.call(cell.parentElement.querySelectorAll('.e-rowcell:not(.e-hide)'));
        return LastCell[LastCell.length - 1] === cell;
    }

    private isLastRow(cell: Element): boolean {
        const LastRow: Element[] = [].slice.call(closest(cell, 'tbody').querySelectorAll('.e-row:not(.e-hiddenrow)'));
        return LastRow[LastRow.length - 1] === cell.parentElement;
    }

    private isFirstRow(cell: Element): boolean {
        const rows: Element[] = [].slice.call(closest(cell, 'tbody').querySelectorAll('.e-row:not(.e-hiddenrow)'));
        return cell.parentElement === rows[0];
    }

    private isFirstCell(cell: Element): boolean {
        const cells: Element[] = [].slice.call(cell.parentElement.querySelectorAll('.e-rowcell:not(.e-hide)'));
        return cells[0] === cell;
    }

    private performDragSelection(startIndex: number, endIndex: number, clearIndex: number): void {
        let sIndex: number = startIndex;
        let eIndex: number = endIndex;
        if (startIndex > endIndex) {
            sIndex = endIndex;
            eIndex = startIndex;
        }
        if (clearIndex !== -1) {
            this.clearSelectionExceptDragIndexes(sIndex, eIndex);
        }
        this.selectRangeOfRows(sIndex, eIndex);
    }

    private selectRangeOfRows(startIndex: number, endIndex: number): void {
        const HeaderRows: NodeListOf<Element> = this.parent.getHeaderContent().querySelectorAll('tr.e-row[data-uid]');
        const ContentRows: NodeListOf<Element> = this.parent.getContent().querySelectorAll('tr.e-row[data-uid]');
        const rows: Element[] = Array.from(HeaderRows).concat(Array.from(ContentRows));
        for (let i: number = startIndex; i <= endIndex; i++) {
            const row: Element = this.parent.options.enableVirtualization ? rows.filter((_: HTMLElement) => parseInt(_.getAttribute('aria-rowindex'), 10) - 1 === i)[0] : rows[parseInt(i.toString(), 10)];
            this.selectRow(row, false);
        }
    }

    private selectRow(row: Element, isMovableRow: boolean): void {
        if (!isNullOrUndefined(row)) {
            row.setAttribute('aria-selected', 'true');
            let cells: Element[] = [].slice.call(row.querySelectorAll('.e-rowcell'));
            if (this.parent.options.allowRowDragAndDrop) {
                cells = ([].slice.call(row.querySelectorAll('.e-rowdragdrop')) as Element[]).concat(cells);
            }
            addRemoveActiveClasses(cells, true, ...['e-aria-selected', 'e-active']);
            if (!isMovableRow) {
                const checkboxCell: Element = cells.filter((cell: Element) => cell.classList.contains('e-gridchkbox'))[0] as Element;
                if (!isNullOrUndefined(checkboxCell) && checkboxCell.classList.contains('e-gridchkbox')) {
                    checkboxCell.querySelector('.e-frame').classList.replace('e-uncheck', 'e-check');
                }
            }
        }
    }

    private clearSelectionByRow(row: Element): void {
        let cells: Element[] = [].slice.call(row.querySelectorAll('.e-rowcell'));
        if (this.parent.options.allowRowDragAndDrop) {
            cells = ([].slice.call(row.querySelectorAll('.e-rowdragdrop')) as Element[]).concat(cells);
        }
        row.removeAttribute('aria-selected');
        addRemoveActiveClasses(cells, false, ...['e-aria-selected', 'e-active']);
        const checkboxCell: Element = cells.filter((cell: Element) => cell.classList.contains('e-gridchkbox'))[0] as Element;
        if (!isNullOrUndefined(checkboxCell) && checkboxCell.classList.contains('e-gridchkbox')) {
            checkboxCell.querySelector('.e-frame').classList.replace('e-check', 'e-uncheck');
        }
    }

    private clearSelectionExceptDragIndexes(startIndex: number, endIndex: number): void {
        const rows: Element[] = this.parent.getRows();
        for (let i: number = 0; i < rows.length; i++) {
            const j: number = this.parent.options.enableVirtualization ? parseInt(rows[parseInt(i.toString(), 10)].getAttribute('aria-rowindex'), 10) - 1 : i;
            if (j < startIndex || j > endIndex) {
                const row: Element = this.parent.options.enableVirtualization ? rows.filter((_: HTMLElement) => parseInt(_.getAttribute('aria-rowindex'), 10) - 1 === j)[0] : rows[parseInt(j.toString(), 10)];
                this.clearSelectionByRow(row);
            }
        }
    }

    private isRowType(): boolean {
        return this.parent.options.selectionMode === 'Row' || this.parent.options.selectionMode === 'Both';
    }

    private isSingleSel(): boolean {
        return this.parent.options.selectionType === 'Single';
    }
}
