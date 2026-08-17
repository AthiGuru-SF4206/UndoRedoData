import { MouseEventArgs, Droppable, removeClass, Draggable, DropEventArgs, createElement, isNullOrUndefined } from '@syncfusion/ej2-base';
import { remove, closest as closestElement, classList, BlazorDragEventArgs, Browser } from '@syncfusion/ej2-base';
import { parentsUntil, getPosition, getScrollBarWidth, removeElement, addRemoveActiveClasses, getRootElement } from './util';
import { SfGrid } from './sf-grid-fn';
import { IPosition } from './interfaces';

// eslint-disable-next-line valid-jsdoc, jsdoc/require-param, jsdoc/require-returns
/**
 *
 * Reorder module is used to handle row reordering.
 *
 * @hidden
 */
export class RowDD {
    //Internal variables
    private startedRow: HTMLTableRowElement;
    private dragTarget: number;
    private timer: number;
    private isOverflowBorder: boolean = true;
    private rowData: Object;
    private dragStartData: Object;
    private draggable: Draggable;
    private droppable: Droppable;
    private borderIndex: number;
    private destinationGrid: SfGrid;
    private istargetGrid: boolean = false;
    private dataRowElements: HTMLElement[] = [];
    private showAddNewRowDisable: boolean = false;
    private cloneElementGrid: HTMLElement | null = null;
    private previousDragTarget: Element = null;
    /**
     * Helper function to handle drag start event.
     *
     * @param {Object} e - The event object containing the sender property with MouseEventArgs.
     */
    /* tslint:disable-next-line:max-line-length */
    // tslint:disable-next-line:max-func-body-length
    private helper: Function = (e: { sender: MouseEventArgs }) => {
        const gObj: SfGrid = this.parent;
        const target: Element = this.draggable.currentStateTarget as Element;
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        this.draggable.queryPositionInfo = function (value: any): any {
            if (gObj.options.enableRtl && isNullOrUndefined(gObj.options.rowDropTarget)) {
                value.left = (this.position.left) - ((this.parentClientRect.left + this.borderWidth.left)) - document.querySelector('.e-cloneproperties').clientWidth + (gObj.element.querySelector('.e-rowdragdrop').clientWidth / 2) + 'px';
            }
            return value;
        };
        const visualElement: HTMLElement = createElement('div', {
            className: 'e-cloneproperties e-draganddrop e-grid e-dragclone',
            styles: 'height:"auto", z-index:2, width:' + gObj.element.offsetWidth
        });
        const table: Element = createElement('table', { styles: 'width:' + gObj.element.offsetWidth });
        const tbody: Element = createElement('tbody');
        const senderElement: Element = e.sender.target as Element;
        const senderTargetIsNotCell: boolean = (isNullOrUndefined(parentsUntil(senderElement, 'e-rowcell')) || parentsUntil(target, 'e-emptyrow')) && !senderElement.classList.contains('e-selectionbackground');
        if (document.getElementsByClassName('e-griddragarea').length ||
            ((senderTargetIsNotCell && gObj.options.selectionType !== 'Single')) ||
            (!gObj.options.rowDropTarget && !parentsUntil(target, 'e-rowdragdrop') && Browser.isDevice)
            || gObj.options.selectionType === 'Single' && (target as Element).parentElement.getAttribute('aria-rowindex') === null
            || this.isTargetInEditMode(target as Element)) {
            if (isNullOrUndefined(parentsUntil(target as Element, 'e-rowdragdrop'))) {
                return false;
            }
        }
        if (gObj.options.rowDropTarget &&
            gObj.options.selectionMode === 'Row' && gObj.options.selectionType === 'Single' &&
            (this.draggable.currentStateTarget as Element).parentElement.getAttribute('aria-rowindex') !== null) {
            gObj.dotNetRef.invokeMethodAsync('SelectRow', parseInt((this.draggable.currentStateTarget as Element).parentElement.getAttribute('aria-rowindex'), 10) - 1, false, -1);
        }
        const targetRow: HTMLTableRowElement = closestElement(target as Element, 'tr') as HTMLTableRowElement;
        if (!targetRow) {
            return false;
        }
        this.startedRow = targetRow.cloneNode(true) as HTMLTableRowElement;
        if (!isNullOrUndefined(this.startedRow.querySelector('.e-rowcell.e-focus'))) {
            removeClass([this.startedRow.querySelector('.e-rowcell.e-focus')], ['e-focus', 'e-focused']);
        }
        const selectedRows: Element[] = gObj.getSelectedRows();
        const targetRowIsDetailRow: HTMLElement = parentsUntil(target, 'e-detailrow') as HTMLElement;
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const rowsSelected: Element[] = selectedRows.filter(function (row: Element): any {
            const rowIsDetailRow: HTMLElement = parentsUntil(row, 'e-detailrow') as HTMLElement;
            return (
                isNullOrUndefined(rowIsDetailRow) ||
                (!isNullOrUndefined(targetRowIsDetailRow) && !isNullOrUndefined(rowIsDetailRow))
            );
        });
        const dragUid: string = this.startedRow.getAttribute('data-uid') ? this.startedRow.getAttribute('data-uid') : null;
        const isDragRow: boolean = (selectedRows as HTMLElement[]).some((row: HTMLElement) => {
            return (row as HTMLElement).getAttribute('data-uid') === dragUid;
        });
        removeElement(this.startedRow, '.e-indentcell');
        removeElement(this.startedRow, '.e-detailrowcollapse');
        removeElement(this.startedRow, '.e-detailrowexpand');
        this.removeCell(this.startedRow, 'e-gridchkbox');
        const exp: RegExp = new RegExp('e-active', 'g'); //high contrast issue
        this.startedRow.innerHTML = this.startedRow.innerHTML.replace(exp, '');
        tbody.appendChild(this.startedRow);

        if (isDragRow && !isNullOrUndefined(rowsSelected) && rowsSelected.length > 1 && gObj.getSelectedRows().length > 1 && this.startedRow.hasAttribute('aria-selected')) {
            const dropCountEle: HTMLElement = createElement('span', {
                className: 'e-dropitemscount', innerHTML: '' + rowsSelected.length
            });
            visualElement.appendChild(dropCountEle);
        }
        const ele: Element = closestElement(target as Element, 'tr').querySelector('.e-icon-rowdragicon');
        if (ele && parentsUntil(target, 'e-rowdragdrop')) {
            ele.classList.add('e-dragstartrow');
        }
        table.appendChild(tbody);
        visualElement.appendChild(table);
        const gridIsDetailRow: Element = parentsUntil(gObj.element, 'e-detailrow') as Element;
        if (gridIsDetailRow) {
            this.cloneElementGrid = gObj.element;
            const mainParent: HTMLElement | null = getRootElement(gObj.element) as HTMLElement;
            mainParent.appendChild(visualElement);
        } else {
            gObj.element.appendChild(visualElement);
        }
        return visualElement;
    }

    private dragStart: Function = (e: { target: HTMLElement, event: MouseEventArgs } & BlazorDragEventArgs) => {
        const gObj: SfGrid = this.parent;
        document.body.classList.add('e-prevent-select');
        if (document.getElementsByClassName('e-griddragarea').length) {
            return;
        }
        const spanCssEle: HTMLSpanElement = this.parent.element.querySelector('.e-dropitemscount') as HTMLSpanElement;
        if (this.parent.getSelectedRows().length > 1 && spanCssEle) {
            spanCssEle.style.left = (document.querySelector('.e-cloneproperties table') as HTMLTableRowElement)
                .offsetWidth - 5 + 'px';
        }
        const fromIdx: number = parseInt(this.startedRow.getAttribute('aria-rowindex'), 10) - 1;
        const dragUid: string = !isNullOrUndefined(this.startedRow.getAttribute('data-uid')) ? this.startedRow.getAttribute('data-uid') : null;
        this.parent.dotNetRef.invokeMethodAsync('RowDragStartEvent', fromIdx, dragUid);
        e.bindEvents(e.dragElement);
        this.dragStartData = this.rowData;
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const dropElem: any = document.getElementById(gObj.options.rowDropTarget);
        if (gObj.options.rowDropTarget && dropElem && dropElem.blazor__instance &&
            (typeof (<{ getModuleName?: Function }>dropElem.blazor__instance).getModuleName === 'function') &&
            (<{ getModuleName?: Function }>dropElem.blazor__instance).getModuleName() === 'grid') {
            if (isNullOrUndefined(gObj.element.querySelector('.e-dragstartrow'))) {
                dropElem.blazor__instance.getContent().classList.add('e-allowRowDrop');
            }
        }
    }

    private drag: Function = (e: { target: HTMLElement, event: MouseEventArgs }) => {
        const gObj: SfGrid = this.parent;
        this.istargetGrid = false;
        this.destinationGrid = this.parent;
        this.showAddNewRowDisable = false;
        const cloneElement: HTMLElement = document.querySelector('.e-cloneproperties') as HTMLElement;
        const target: Element = this.getElementFromPosition(cloneElement, e.event);
        this.previousDragTarget = target;
        const cloneElementGrid: HTMLElement | null = !isNullOrUndefined(this.cloneElementGrid) ?
            this.cloneElementGrid : (parentsUntil(cloneElement, 'e-grid') as HTMLElement).parentElement || null;
        const cloneElementDragRow: HTMLElement | null = !isNullOrUndefined(cloneElementGrid) ? cloneElementGrid.querySelector('.e-dragstartrow') as HTMLElement : null;
        const targetElementGrid: HTMLElement = parentsUntil(e.target, 'e-grid') as HTMLElement;
        if (this.parent.options.rowDropTarget) {
            const dropElement: HTMLElement | null = document.getElementById(gObj.options.rowDropTarget);
            if (!isNullOrUndefined(cloneElementGrid) && !isNullOrUndefined(targetElementGrid) && cloneElementGrid.id !==
                targetElementGrid.id) {
                this.destinationGrid = (isNullOrUndefined((dropElement)) ||
                    // eslint-disable-next-line @typescript-eslint/no-explicit-any
                    isNullOrUndefined((<any>dropElement).blazor__instance)) ?
                    // eslint-disable-next-line @typescript-eslint/no-explicit-any
                    this.parent : (<any>dropElement).blazor__instance;
            }

            if (parentsUntil(e.target, 'e-grid')) {
                this.istargetGrid = this.parent.options.rowDropTarget === parentsUntil(e.target, 'e-grid').id;
            }
        }
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const instance = !isNullOrUndefined(targetElementGrid) && !isNullOrUndefined((targetElementGrid as any).blazor__instance)
            ? (targetElementGrid as any).blazor__instance : null;

        if (!isNullOrUndefined(instance) && !isNullOrUndefined(instance.options) && instance.options.showAddNewRow && !this.showAddNewRowDisable) {
            gObj.dotNetRef.invokeMethodAsync('DisableShowAddForm', 'RowDragStart', false, this.destinationGrid.dotNetRef);
            this.showAddNewRowDisable = true;
        }

        classList(cloneElement, ['e-defaultcur'], ['e-notallowedcur', 'e-movecur']);
        this.isOverflowBorder = true;
        const trElement: HTMLTableRowElement = parentsUntil(target, 'e-grid') ? closestElement(e.target, 'tr') as HTMLTableRowElement : null;
        const cloneIsDetailRow: Element = parentsUntil(cloneElementGrid, 'e-detailrow');
        const targetIsDetailRow: Element = parentsUntil(targetElementGrid, 'e-detailrow');
        const targetRow: HTMLTableRowElement = parentsUntil(target, 'e-row') as HTMLTableRowElement;
        const targetIsLastRowDragBorder: boolean = !isNullOrUndefined(target) && target.classList.contains('e-lastrow-dragborder');
        if (!e.target) { return; }
        this.stopTimer();
        gObj.element.classList.add('e-rowdrag');
        this.dragTarget = trElement && parentsUntil(target, 'e-grid').id === (isNullOrUndefined(cloneElementGrid) ? cloneElement.parentElement.id : cloneElementGrid.id) ?
            gObj.options.groupCount > 0 || this.parent.options.showAddNewRow ? parseInt(trElement.getAttribute('aria-rowindex'), 10) - 1 : trElement.rowIndex : parseInt(this.startedRow.getAttribute('aria-rowindex'), 10) - 1;
        if (gObj.options.rowDropTarget && !isNullOrUndefined(targetElementGrid) && cloneElementGrid.id !== targetElementGrid.id &&
            !isNullOrUndefined(cloneElementDragRow)) {
            this.dragTarget = trElement && parentsUntil(target, 'e-grid').id !== (!isNullOrUndefined(this.cloneElementGrid) ? cloneElementGrid.id : cloneElement.parentElement.id) ? trElement.rowIndex :
                parseInt(this.startedRow.getAttribute('aria-rowindex'), 10) - 1;
        }
        if (gObj.options.rowDropTarget) {
            const dropElement: HTMLElement = document.getElementById(gObj.options.rowDropTarget) as HTMLElement;
            const gridRow: Element = parentsUntil(e.target, 'e-row');
            const targetISGrid: Element = parentsUntil(target, 'e-grid');
            if (parentsUntil(target, 'e-gridcontent')) {
                if (cloneElementGrid.id === parentsUntil(target, 'e-grid').id && (!isNullOrUndefined(targetRow) && targetRow.getAttribute('aria-selected') === 'true')
                    || (!gObj.options.allowEmptyAreaDrop && isNullOrUndefined(gridRow) && !isNullOrUndefined(targetElementGrid) && isNullOrUndefined(targetElementGrid.querySelector('.e-emptyrow')))
                    || (!isNullOrUndefined(dropElement) && dropElement.classList.contains('e-grid') &&
                        !isNullOrUndefined(targetElementGrid) && this.destinationGrid.element.id !== targetElementGrid.id)
                    || (targetIsLastRowDragBorder)) {
                    classList(cloneElement, ['e-notallowedcur'], ['e-defaultcur']);
                }
                else if (parentsUntil(trElement, 'e-showAddNewRow') || !isNullOrUndefined(targetISGrid) && targetISGrid.parentElement.classList.contains('e-treegrid')) {
                    classList(cloneElement, ['e-notallowedcur'], ['e-defaultcur']);
                }
                else {
                    classList(cloneElement, ['e-defaultcur'], ['e-notallowedcur']);
                }
            } else if (parentsUntil(target, 'e-droppable:not(.e-headercontent)')) {
                classList(cloneElement, ['e-defaultcur'], ['e-notallowedcur']);
            }
            else if (!isNullOrUndefined(dropElement) && !isNullOrUndefined(target) && (parentsUntil(target, 'e-grid') || !dropElement.contains(target))) {
                classList(cloneElement, ['e-notallowedcur'], ['e-defaultcur']);
            }
        } else {
            const elem: Element = parentsUntil(target, 'e-grid');
            if (elem && elem.id === (!isNullOrUndefined(this.cloneElementGrid) ? cloneElementGrid.id : cloneElement.parentElement.id)
                && !isNullOrUndefined(targetRow) && (targetRow.getAttribute('aria-selected') === 'false'
                    || isNullOrUndefined(targetRow.getAttribute('aria-selected')))) {
                classList(cloneElement, ['e-movecur'], ['e-defaultcur']);
            }
            else if (targetIsLastRowDragBorder) {
                classList(cloneElement, ['e-notallowedcur'], ['e-defaultcur']);
            } else if (gObj.options.allowEmptyAreaDrop && target && target.classList.contains('e-content') && !(gObj.options.groupCount > 0)){
                classList(cloneElement, ['e-movecur'], ['e-defaultcur']);
            } else {
                classList(cloneElement, ['e-notallowedcur'], ['e-movecur']);
            }
        }
        const stackedColumn: number = gObj.reorderModule.processStackedColumns(
            gObj.reorderModule.getColumnsModel(gObj.options.columns)).length;
        const stackedHeader: number = gObj.getStackedColumns(
            gObj.reorderModule.getColumnsModel(gObj.options.columns)).length;
        if (!isNullOrUndefined(targetElementGrid) && gObj.options.allowGrouping
            && ((<any>targetElementGrid).blazor__instance.options.groupCount === gObj.options.columns.length
                || (stackedColumn - stackedHeader) === gObj.options.groupCount) && !gObj.options.showGroupedColumn) {
            classList(cloneElement, ['e-notallowedcur'], ['e-defaultcur']);
        }

        if (gObj.options.allowRowDragAndDrop ||
            (!gObj.options.rowDropTarget && e.target.classList.contains('e-selectionbackground'))) {
            if (parentsUntil(target, 'e-grid')) {
                const treeGridElement: HTMLElement = !isNullOrUndefined(cloneElementGrid) ? parentsUntil(cloneElementGrid, 'e-treegrid') as HTMLElement : null;
                if ((!isNullOrUndefined(targetElementGrid) && cloneElementGrid ===
                    targetElementGrid || treeGridElement)) {
                    if (!isNullOrUndefined(trElement) && !parentsUntil(trElement, 'e-content')) {
                        // eslint-disable-next-line @typescript-eslint/no-explicit-any
                        this.removeTargetGridBorder((<any>cloneElementGrid).blazor__instance);
                        // eslint-disable-next-line @typescript-eslint/no-explicit-any
                        this.removeTargetGridBorder((<any>targetElementGrid).blazor__instance);
                    }
                    this.updateScrollPostion(e.event, targetRow);
                }
            }
            if (!isNullOrUndefined(document.querySelector('.e-lastrow-dragborder'))) {
                document.querySelector('.e-lastrow-dragborder').remove();
            }
            if (parentsUntil(trElement, 'e-columnheader')) {
                const detailRow: Element = parentsUntil(trElement, 'e-detailrow');
                if (!isNullOrUndefined(detailRow) && detailRow.getElementsByClassName('e-emptyrow')) {
                    classList(cloneElement, ['e-notallowedcur'], ['e-defaultcur']);
                    return;
                }
            }
            const isNotEmptyGrid: boolean = !isNullOrUndefined(trElement) && !trElement.classList.contains('e-emptyrow');
            const isFrozenRowsAndColumn: boolean = this.parent.options.frozenRows && this.parent.options.frozenColumns && !isNullOrUndefined(trElement) && parseInt(this.startedRow.getAttribute('aria-rowindex'), 10) - 1 !== parseInt(trElement.getAttribute('aria-rowindex'), 10) - 1;
            if (this.isOverflowBorder && ((parseInt(this.startedRow.getAttribute('aria-rowindex'), 10) - 1 !== this.dragTarget) || isFrozenRowsAndColumn) && isNotEmptyGrid) {
                this.moveDragRows(e, this.startedRow, trElement);
            } else {
                const rows: Element[] = this.parent.getRows();
                const isLastRow: boolean = !isNullOrUndefined(trElement) && this.startedRow.getAttribute('data-uid') !== rows[(rows.length - 1)].getAttribute('data-uid');
                if (trElement && !isNullOrUndefined(this.parent.getRowByIndex(rows.length - 1)) &&
                    this.parent.getRowByIndex(rows.length - 1).getAttribute('data-uid') ===
                    trElement.getAttribute('data-uid') && isLastRow && !(gObj.options.groupCount)) {
                    const bottomborder: HTMLElement = createElement('div', { className: 'e-lastrow-dragborder' });
                    const gridcontentEle: Element = this.parent.getContent();
                    if (!isNullOrUndefined(gridcontentEle) && !isNullOrUndefined(parentsUntil(trElement, 'e-row'))) {
                        const isHorizontalScrollAlone: boolean = gridcontentEle.scrollWidth > gridcontentEle.clientWidth &&
                            gridcontentEle.scrollHeight <= gridcontentEle.clientHeight;
                        bottomborder.style.width = isHorizontalScrollAlone ? (this.parent.getContent() as HTMLElement).offsetWidth + 'px' : (this.parent.getContent() as HTMLElement).offsetWidth - this.getScrollWidth() + 'px';
                        this.borderIndex = Number(parentsUntil(trElement, 'e-row').getAttribute('aria-rowindex')) - 1;
                        if (!gridcontentEle.parentElement.querySelectorAll('.e-lastrow-dragborder').length) {
                            gridcontentEle.classList.add('e-grid-relative');
                            gridcontentEle.parentElement.appendChild(bottomborder);
                            this.setBottomBorderPosition(gObj, bottomborder);
                        }
                    }
                }
                else if (!isNullOrUndefined(targetElementGrid) && targetElementGrid.querySelector('.e-emptyrow') && cloneElementGrid.id !== targetElementGrid.id && targetElementGrid.querySelector('.e-content').getElementsByClassName('e-emptyrow').length > 0 && !cloneElement.classList.contains('e-notallowedcur')) {
                    // eslint-disable-next-line @typescript-eslint/no-explicit-any
                    if ((<any>targetElementGrid).blazor__instance.getRows().length === 0 || !isNullOrUndefined(trElement) && parentsUntil(trElement, 'e-detailrow') && trElement.getElementsByClassName('e-emptyrow').length > 0) {
                        if (!isNullOrUndefined(cloneIsDetailRow) || (isNullOrUndefined(cloneIsDetailRow) &&
                            isNullOrUndefined(targetIsDetailRow))) {
                            const bottomborder: HTMLElement = createElement('div', { className: 'e-lastrow-dragborder' });
                            let gridcontentEle: Element = targetElementGrid.querySelector('.e-content');
                            if (!isNullOrUndefined(gridcontentEle)) {
                                const isHorizontalScrollAlone: boolean = gridcontentEle.scrollWidth > gridcontentEle.clientWidth &&
                                    gridcontentEle.scrollHeight <=
                                    gridcontentEle.clientHeight;
                                bottomborder.style.width = isHorizontalScrollAlone ? (this.destinationGrid.getContent() as HTMLElement).offsetWidth + 'px' : (this.destinationGrid.getContent() as HTMLElement).offsetWidth - this.getScrollWidth() + 'px';
                                const isDetailRow: boolean = !isNullOrUndefined(trElement) ? trElement.classList.contains('e-detailrow') : false;
                                gridcontentEle = isDetailRow ? trElement.getElementsByClassName('e-grid')[0].querySelector('.e-content') : gridcontentEle;
                                bottomborder.style.position = isDetailRow ? 'relative' : bottomborder.style.position;
                                if (!gridcontentEle.parentElement.querySelectorAll('.e-lastrow-dragborder').length) {
                                    gridcontentEle.classList.add('e-grid-relative');
                                    gridcontentEle.parentElement.appendChild(bottomborder);
                                    bottomborder.style.bottom = this.destinationGrid.options.allowPaging ? ((this.destinationGrid.element.querySelector('.e-pager') as HTMLElement).offsetHeight + this.getScrollWidth()) + 'px' : this.getScrollWidth() + 'px';
                                }
                            }
                        }
                    }
                }
                else if (!isNullOrUndefined(targetElementGrid) && gObj.options.allowEmptyAreaDrop
                    && (<any>targetElementGrid).blazor__instance.options.groupCount === 0) {
                    const gridContentEle: Element = targetElementGrid.querySelector('.e-content');
                    const bottomborder: HTMLElement = createElement('div', { className: 'e-lastrow-dragborder' });
                    const dataRows: NodeListOf<Element>  = gridContentEle.querySelectorAll('tr.e-row:not(.e-emptyrow)');
                    const lastDataRow: Element = dataRows[dataRows.length - 1];
                    if (lastDataRow && !cloneElement.classList.contains('e-notallowedcur')) {
                        const lastRowRect: ClientRect = lastDataRow.getBoundingClientRect();
                        const contentRect: ClientRect = gridContentEle.getBoundingClientRect();
                        if (!gridContentEle.querySelector('.e-lastrow-dragborder')) {
                            bottomborder.style.position = 'absolute';
                            bottomborder.style.left = '0px';
                            const isHorizontalScrollAlone: boolean = gridContentEle.scrollWidth > gridContentEle.clientWidth &&
                                    gridContentEle.scrollHeight <=
                                    gridContentEle.clientHeight;
                            bottomborder.style.width = isHorizontalScrollAlone ? (this.destinationGrid.getContent() as HTMLElement).offsetWidth + 'px' : (this.destinationGrid.getContent() as HTMLElement).offsetWidth - this.getScrollWidth() + 'px';
                            const scrollOffset: number = gridContentEle.scrollTop;
                            const offsetTop: number = lastRowRect.bottom - contentRect.top + scrollOffset;
                            bottomborder.style.top = offsetTop + 'px';
                            gridContentEle.classList.add('e-grid-relative');
                            gridContentEle.appendChild(bottomborder);
                        }
                    }
                }
                this.removeBorder(trElement);
            }

            if (gObj.options.allowGrouping && gObj.options.groupCount) {

                if (!isNullOrUndefined(trElement) && !(trElement.querySelector('td.e-groupcaption')) && (this.startedRow.getAttribute('caption-uid') !== trElement.getAttribute('caption-uid')) && gObj.options.groupCount) {

                    this.addorRemoveDashedBorder(e, false, this.dataRowElements);
                    this.dataRowElements = this.parent.getDataRows().filter((row: Element) => {
                        return row.getAttribute('caption-uid') === trElement.getAttribute('caption-uid');
                    }) as HTMLElement[];
                    this.addorRemoveDashedBorder(e, true, this.dataRowElements);
                }
                else {
                    this.addorRemoveDashedBorder(e, false, this.dataRowElements);
                }
                const stackedColumn: number =
                    gObj.reorderModule.processStackedColumns(gObj.reorderModule.getColumnsModel(gObj.options.columns)).length;
                const stackedHeader: number = gObj.getStackedColumns(gObj.reorderModule.getColumnsModel(gObj.options.columns)).length;
                if ((gObj.options.groupCount === gObj.options.columns.length ||
                    ((stackedColumn - stackedHeader) === gObj.options.groupCount)) && !gObj.options.showGroupedColumn) {
                    this.addorRemoveDashedBorder(e, false, this.dataRowElements);
                }
            }

            if (!this.isOverflowBorder) {
                this.addorRemoveDashedBorder(e, false, this.dataRowElements);
            }
        }
        if (gObj.options.rowDropTarget && this.istargetGrid) {
            if (cloneElementGrid !== targetElementGrid) {
                this.updateScrollPostion(e.event, targetRow);
                this.moveDragRows(e, this.startedRow, trElement);
                if (!isNullOrUndefined(trElement)) {
                    this.lastRowBorderBetweenGrids(trElement, e.event);
                }
            }
        }
    }

    private lastRowBorderBetweenGrids: Function = (trElement: HTMLElement, event: MouseEventArgs) => {
        const gObj: SfGrid = this.destinationGrid;
        const rowElements: Element[] = gObj.getRows();
        const isLastRow: boolean = rowElements.length > 0 && !isNullOrUndefined(trElement) && gObj.getContent().querySelector('tr:last-child') === trElement;
        //Get the mouse position relative to the target row
        const mouseY: number = getPosition(event).y - trElement.getBoundingClientRect().top;
        //Calculate the midpoint of the target row
        const targetMidpoint: number = trElement.offsetHeight / 2;
        //Calculate the combined height of the rows
        const rowsHeight: number = Array.from(rowElements).reduce((totalHeight: number, row: Element) => {
            return totalHeight + (row as HTMLElement).offsetHeight;
        }, 0);
        //Check if there is empty space after the last row
        const checkEmptySpace: boolean = rowsHeight >= gObj.getContent().clientHeight;
        if (rowElements.length > 0 && mouseY > targetMidpoint && checkEmptySpace && trElement && !isNullOrUndefined(gObj.getRowByIndex(rowElements.length - 1)) && gObj.getRowByIndex(rowElements.length - 1).getAttribute('data-uid') === trElement.getAttribute('data-uid') && isLastRow && !gObj.options.groupCount) {
            const bottomborder: HTMLElement = createElement('div', { className: 'e-lastrow-dragborder' });
            const gridcontentEle: HTMLElement = gObj.getContent() as HTMLElement;
            if (!isNullOrUndefined(gridcontentEle)) {
                const isHorizontalScrollAlone: boolean = gridcontentEle.scrollWidth > gridcontentEle.clientWidth &&
                    gridcontentEle.scrollHeight <= gridcontentEle.clientHeight;
                bottomborder.style.width = isHorizontalScrollAlone ? gridcontentEle.offsetWidth + 'px' : gridcontentEle.offsetWidth - this.getScrollWidth() + 'px';
                this.borderIndex = Number(parentsUntil(trElement, 'e-row').getAttribute('aria-rowindex')) - 1;
                if (!gridcontentEle.parentElement.querySelectorAll('.e-lastrow-dragborder').length) {
                    gridcontentEle.classList.add('e-grid-relative');
                    gridcontentEle.parentElement.appendChild(bottomborder);
                    this.setBottomBorderPosition(gObj, bottomborder);
                }
            }
        }
    }

    private setBottomBorderPosition: Function = (gObj: SfGrid,  bottomBorder: HTMLElement) => {
        // Retrieve relevant DOM elements
        const aggregateFooterElement: HTMLElement | null = gObj.element.querySelector('.e-gridfooter') as HTMLElement;
        const pagerElement: HTMLElement | null = gObj.element.querySelector('.e-pager') as HTMLElement;
        // Determine heights of the footer and pager, setting default to 0 if elements are not found
        const footerOffsetHeight: number = aggregateFooterElement ? aggregateFooterElement.offsetHeight : 0;
        const pagerOffsetHeight: number = this.parent.options.allowPaging && pagerElement ? pagerElement.offsetHeight : 0;
        // Calculate the total height for the bottom border position
        const scrollWidth: number = this.getScrollWidth();
        const bottomOffset: number = footerOffsetHeight + pagerOffsetHeight + scrollWidth;
        // Apply calculated position to the bottom border style
        bottomBorder.style.bottom = `${bottomOffset}px`;
    }

    private dragStop: Function = (e: { target: HTMLTableRowElement, event: MouseEventArgs, helper: Element }) => {
        document.body.classList.remove('e-prevent-select');
        this.processDragStop(e);
    }

    private emptyTargertInGrid: Function = (target: HTMLElement) => {
        const gObj: SfGrid = this.parent;
        const contentDragBorder: HTMLCollectionOf<Element> = gObj.getContent().getElementsByClassName('e-dragborder');
        const headerDragBorder: HTMLCollectionOf<Element> = gObj.getHeaderContent().getElementsByClassName('e-firstrow-dragborder');
        const lastRowDragBorder: HTMLCollectionOf<Element> = gObj.getContent().parentElement.getElementsByClassName('e-lastrow-dragborder');
        const iconTarget: boolean = !target.classList.contains('e-rowdragdrop') && !target.classList.contains('e-icon-rowdragicon');
        const emptyTarget: boolean = this.destinationGrid.getRows().length > 0 && !isNullOrUndefined(gObj.element.querySelector('.e-dragstartrow')) && !isNullOrUndefined(contentDragBorder) && !isNullOrUndefined(headerDragBorder) && contentDragBorder.length === 0 && headerDragBorder.length === 0 && isNullOrUndefined(parentsUntil(target, 'e-rowcell')) && iconTarget;
        if (!this.parent.options.rowDropTarget && !emptyTarget && !this.isOverflowBorder && !isNullOrUndefined(headerDragBorder) &&
            !isNullOrUndefined(lastRowDragBorder) && (headerDragBorder.length === 0 && lastRowDragBorder.length === 0)) {
            return true;
        }
        return emptyTarget;
    }

    private processDragStop: Function = (e: { target: HTMLTableRowElement, event: MouseEventArgs, helper: Element }) => {
        const gObj: SfGrid = this.parent;
        const targetEle: Element = this.getElementFromPosition(e.helper as HTMLElement, e.event);
        const target: Element = targetEle && !targetEle.classList.contains('e-dlg-overlay') ?
            targetEle : e.target;
        gObj.element.classList.remove('e-rowdrag');
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const dropElement: any = document.getElementById(gObj.options.rowDropTarget);
        const dragStartRow: Element = gObj.element.querySelector('.e-dragstartrow');
        const cloneElement: HTMLElement = document.querySelector('.e-cloneproperties') as HTMLElement;
        const cloneElementGrid: HTMLElement = !isNullOrUndefined(this.cloneElementGrid) ? this.cloneElementGrid : (parentsUntil(cloneElement, 'e-grid') as HTMLElement).parentElement;
        const targetElementGrid: HTMLElement = parentsUntil(e.target, 'e-grid') as HTMLElement;
        const targetIsNotGrid: boolean = !isNullOrUndefined(dropElement) && !dropElement.classList.contains('e-grid') && !isNullOrUndefined(targetElementGrid) && cloneElementGrid.id !== targetElementGrid.id;
        const whiteSpaceInEmptyGrid: boolean = !isNullOrUndefined(targetElementGrid) && !isNullOrUndefined(targetElementGrid.querySelector('.e-emptyrow')); //When a row is dropped into an empty grid that displays 'no records to display,' and the target is a content element, then the variable becomes true.
        if (this.parent.options.allowRowDragAndDrop && this.parent.options.rowDropTarget && ((!parentsUntil(target, 'e-grid') && dropElement.contains(e.target)) || (targetIsNotGrid && (parentsUntil(e.target, 'e-row') || whiteSpaceInEmptyGrid)))) {
            const toIdx: number = 0;
            const targetClass: string = this.getElementXPath(target as HTMLTableRowElement);
            const targetID: string = target.id;
            const fromIdx: number = parseInt(this.startedRow.getAttribute('aria-rowindex'), 10) - 1;
            const positions: ClientRect = target.getBoundingClientRect();
            gObj.dotNetRef.invokeMethodAsync('ReorderRows', fromIdx, toIdx, 'add', false, targetClass, targetID, positions, null, true, false, null, null, false, getPosition(e.event).x, getPosition(e.event).y);
        }
        if (gObj.options.rowDropTarget && dropElement && dropElement.blazor__instance &&
            (typeof (<{ getModuleName?: Function }>dropElement.blazor__instance).getModuleName === 'function') &&
            (<{ getModuleName?: Function }>dropElement.blazor__instance).getModuleName() === 'grid') {
            dropElement.blazor__instance.getContent().classList.remove('e-allowRowDrop');
        }

        //To handle the target differences during dragging and dropping caused by the added lastRowBorder, we allow the row to be dropped in this scenario.
        if (gObj.options.rowDropTarget && this.previousDragTarget !== e.target && target.classList.contains('e-lastrow-dragborder') && !isNullOrUndefined(targetElementGrid) && cloneElementGrid.id !== targetElementGrid.id) {
            this.stopTimer();
            this.parent.getContent().classList.remove('e-grid-relative');
            this.removeBorder(targetEle);
            if (gObj.options.groupCount) {
                this.addorRemoveDashedBorder(e, false, this.dataRowElements);
            }
            this.drop(e);
            return;
        }

        if (!parentsUntil(target, 'e-gridcontent') || cloneElement.classList.contains('e-notallowedcur') || (this.emptyTargertInGrid(target) && !(gObj.options.allowEmptyAreaDrop)) || parentsUntil(e.target, 'e-columnheader')) {
            this.dragTarget = null;
            remove(e.helper);
            this.stopTimer();
            this.removeBorder(targetEle);
            if (this.parent.options.rowDropTarget && !isNullOrUndefined(targetElementGrid)) {
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                this.removeTargetGridBorder((<any>targetElementGrid).blazor__instance);
            }
            if (gObj.options.groupCount) {
                this.addorRemoveDashedBorder(e, false, this.dataRowElements);
            }
            if (dragStartRow) {
                dragStartRow.classList.remove('e-dragstartrow');
            }
            if (gObj.options.showAddNewRow && this.showAddNewRowDisable) {
                gObj.dotNetRef.invokeMethodAsync('DisableShowAddForm', 'RowDragStop', true, null);
                this.showAddNewRowDisable = false;
            }
            const fromIdx: number = parseInt(this.startedRow.getAttribute('aria-rowindex'), 10) - 1;
            if (!isNullOrUndefined(cloneElement) && cloneElement.classList.contains('e-notallowedcur')) {
                gObj.dotNetRef.invokeMethodAsync('ReorderRows', fromIdx, 0, 'add', false, '', target.id, null, null, true, false, null, null, false, getPosition(e.event).x, getPosition(e.event).y);
            }
            return;
        }
        if (this.parent.options.allowRowDragAndDrop) {
            this.stopTimer();
            this.parent.getContent().classList.remove('e-grid-relative');
            this.removeBorder(targetEle);
            if (gObj.options.groupCount) {
                this.addorRemoveDashedBorder(e, false, this.dataRowElements);
            }
            if (dragStartRow && this.parent.options.rowDropTarget && !isNullOrUndefined(targetElementGrid) && cloneElementGrid.id
                !== targetElementGrid.id) {
                return;
            }
            if (dragStartRow && !isNullOrUndefined(targetElementGrid) && cloneElementGrid.id === targetElementGrid.id) {
                dragStartRow.classList.remove('e-dragstartrow');
            }

            const targetClass: string = this.getElementXPath(e.target);
            const targetID: string = target.id;
            const fromIdx: number = parseInt(this.startedRow.getAttribute('aria-rowindex'), 10) - 1;
            const fromUid: string = !isNullOrUndefined(this.startedRow.getAttribute('data-uid')) ? this.startedRow.getAttribute('data-uid') : null;
            let toUid: string | null = !isNullOrUndefined(closestElement(e.target, 'tr') as HTMLTableRowElement) ? (closestElement(e.target, 'tr') as HTMLTableRowElement).getAttribute('data-uid') : null;
            let toIdx: number = this.parent.options.enableVirtualization ? fromIdx ===
                this.dragTarget ? this.dragTarget : fromIdx < this.borderIndex ? this.borderIndex : this.borderIndex + 1 :
                this.parent.options.frozenRows ? fromIdx < this.borderIndex ? this.borderIndex : this.borderIndex + 1 : this.dragTarget;
            if (e.target && e.target.classList.contains('e-content') && gObj.options.allowEmptyAreaDrop) {
                const lastrow: Element | null = gObj.getContentTable().querySelector('tr:last-child');
                if (lastrow && !isNullOrUndefined(cloneElementGrid) &&
                    !isNullOrUndefined(targetElementGrid) && cloneElementGrid.id === targetElementGrid.id) {
                    toIdx = (parseInt(lastrow.getAttribute('aria-rowindex'), 10) - 1);
                    toUid = lastrow.getAttribute('data-uid');
                }
            }
            if (Number.isNaN(toIdx) || isNullOrUndefined(toIdx)) {
                if (gObj.options.showAddNewRow && this.showAddNewRowDisable) {
                    gObj.dotNetRef.invokeMethodAsync('DisableShowAddForm', 'RowDragStop', true, null);
                    this.showAddNewRowDisable = false;
                }
                return;
            }
            setTimeout(() => {
                if (fromIdx !== toIdx) {
                    gObj.dotNetRef.invokeMethodAsync('ReorderRows', fromIdx, toIdx, 'delete', true, targetClass, targetID, null, null, false, false, fromUid, toUid, false, getPosition(e.event).x, getPosition(e.event).y);
                }
            }, 10);
            this.dragTarget = null;
        }
        if (gObj.options.showAddNewRow && this.showAddNewRowDisable) {
            gObj.dotNetRef.invokeMethodAsync('DisableShowAddForm', 'RowDragStop', false, null);
            this.showAddNewRowDisable = false;
        }
    }

    private removeCell: Function = (targetRow: HTMLTableRowElement, className: string) => {
        return [].slice.call(targetRow.querySelectorAll('td')).filter((cell: HTMLTableCellElement) => {
            if (cell.classList.contains(className)) { (targetRow as HTMLTableRowElement).deleteCell(cell.cellIndex); }
        });
    }

    //Module declarations
    private parent: SfGrid;

    /**
     * Constructor for the Grid print module
     *
     * @param {SfGrid} [parent] - Optional parent grid instance.
     * @hidden
     */
    constructor(parent?: SfGrid) {
        this.parent = parent;
        if (this.parent.options.allowRowDragAndDrop) {
            this.initializeDrag();
        }
    }

    private stopTimer(): void {
        window.clearInterval(this.timer);
    }

    public initializeDrag(): void {
        const gObj: SfGrid = this.parent;
        this.draggable = new Draggable(gObj.getContent() as HTMLElement, {
            dragTarget: '.e-rowcelldrag, .e-rowdragdrop, .e-rowcell',
            distance: 5,
            helper: this.helper,
            dragStart: this.dragStart,
            drag: this.drag,
            dragStop: this.dragStop,
            isPreventSelect: false
        });
        this.droppable = new Droppable(gObj.getContent() as HTMLElement, {
            accept: '.e-dragclone',
            drop: this.drop as (e: DropEventArgs) => void
        });
    }

    private updateScrollPostion(e: MouseEvent | TouchEvent, targetRow: HTMLTableRowElement): void {
        const y: number = getPosition(e).y;
        const scrollElement: Element = this.destinationGrid.getContent();
        const clientRect: ClientRect = scrollElement.getBoundingClientRect();
        const rowHeight: number = this.destinationGrid.getRowHeight() - 15;
        if (clientRect.top + rowHeight >= y || (clientRect.top + scrollElement.clientHeight - rowHeight - 20 <= y)) {
            const scrollPixel: number = clientRect.top + rowHeight >= y ?
                -(this.destinationGrid.getRowHeight()) : this.destinationGrid.getRowHeight();
            this.isOverflowBorder = false;
            this.stopTimer();
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            this.timer = (window as any).setInterval(
                () => { this.setScrollDown(scrollElement, scrollPixel, targetRow); }, 200);
        }
    }

    private setScrollDown(scrollElement: Element, scrollPixel: number, targetRow: HTMLTableRowElement): void {
        scrollElement.scrollTop = scrollElement.scrollTop + scrollPixel;
        if (!isNullOrUndefined(targetRow)) {
            const targetRowIndex: number = parseInt(targetRow.getAttribute('aria-rowindex'), 10) - 1;
            const lastRowIndex: number = scrollElement.querySelectorAll('tr.e-row:not(.e-emptyrow)').length - 1;
            const rowElement: Element[] = [].slice.call(scrollElement.querySelectorAll('.e-dragborder'));
            if (targetRowIndex !== lastRowIndex && rowElement.length > 0) {
                addRemoveActiveClasses(rowElement, false, 'e-dragborder');
            }
        }
    }

    private moveDragRows(e: { target: HTMLElement, event: MouseEventArgs }, startedRow: HTMLTableRowElement, targetRow: HTMLTableRowElement)
        : void {
        const cloneElement: HTMLElement = document.querySelector('.e-cloneproperties') as HTMLElement;
        const element: HTMLTableRowElement = closestElement(e.target, 'tr') as HTMLTableRowElement;
        if (parentsUntil(element, 'e-gridcontent') && ((!isNullOrUndefined(cloneElement) && ((!isNullOrUndefined(this.cloneElementGrid) ? this.cloneElementGrid.id
            : (parentsUntil(cloneElement.parentElement as HTMLElement, 'e-grid') as HTMLElement).id) ===
            parentsUntil(element, 'e-grid').id)) || this.istargetGrid)) {
            const targetElement: HTMLTableRowElement = element ?
                element : this.startedRow;
            this.setBorder(targetElement, e.event, startedRow, targetRow);
        }
    }

    private setBorder(element: Element, event: MouseEventArgs, startedRow: HTMLTableRowElement, targetRow: HTMLTableRowElement): void {
        let node: Element = this.parent.element as Element;
        if (this.istargetGrid) {
            node = this.destinationGrid.element as Element;
        }
        const cloneElement: HTMLElement = document.querySelector('.e-cloneproperties') as HTMLElement;
        if (this.parent.element.id !== this.destinationGrid.element.id && (this.parent.element.getElementsByClassName('e-firstrow-dragborder').length > 0 || this.parent.element.getElementsByClassName('e-lastrow-dragborder').length > 0)) {
            this.removeTargetGridBorder(this.parent);
        }
        if (this.parent.options.groupCount) {
            this.removeBorder(element);
        }
        else {
            this.removeFirstRowBorder(element);
            this.removeLastRowBorder(element);
        }
        const dragIconIsActive: Element | null = this.parent.element.querySelector('.e-dragstartrow');
        // Get the mouse position relative to the target row
        const mouseY: number = getPosition(event).y - targetRow.getBoundingClientRect().top;
        // Calculate the midpoint of the target row
        const targetMidpoint: number = targetRow.offsetHeight / 2;
        if (!isNullOrUndefined(targetRow)) {
            const targetRowGrid: HTMLElement = parentsUntil(targetRow, 'e-grid') as HTMLElement;
            if (!isNullOrUndefined(targetRowGrid) && targetRowGrid.id === parentsUntil(startedRow, 'e-grid').parentElement.id &&
                (targetRow.getAttribute('aria-selected') === 'true')) {
                this.removeBorder(element);
                return;
            }
        }
        if (parentsUntil(element, 'e-gridcontent') && (((!isNullOrUndefined(this.cloneElementGrid) ? this.cloneElementGrid.id : parentsUntil(cloneElement.parentElement, 'e-grid').id) ===
            parentsUntil(element, 'e-grid').id) || this.istargetGrid)) {
            removeClass(node.querySelectorAll('.e-rowcell,.e-rowdragdrop,.e-detailrowcollapse, .e-detailrowexpand'), ['e-dragborder']);
            let rowElement: HTMLElement[] = [];
            let targetRowIndex: number = parseInt(targetRow.getAttribute('aria-rowindex'), 10) - 1;
            const lastRow: HTMLElement = this.destinationGrid.getContentTable().querySelector('tr:last-child');
            const addNewRowElement: HTMLTableRowElement = this.destinationGrid.element.querySelector('.e-showAddNewRow') as HTMLTableRowElement;
            if (targetRow && targetRowIndex === 0 && isNullOrUndefined(addNewRowElement) &&
                (this.parent.element.id !== this.destinationGrid.element.id &&
                    mouseY < targetMidpoint || this.parent.element.id === this.destinationGrid.element.id)) {
                if (!targetRow.classList.contains('e-emptyrow') && targetRow.classList.contains('e-row')) {
                    if (this.parent.options.groupCount && !targetRow.classList.contains('e-groupcaption')) {
                        element = targetRow;
                        rowElement = [].slice.call(element.querySelectorAll('.e-groupcaption,.e-summarycell,.e-rowcell,.e-rowdragdrop,.e-detailrowcollapse, .e-detailrowexpand'));
                    }
                    else {
                        const div: HTMLElement = createElement('div', { className: 'e-firstrow-dragborder' });
                        const gridheaderEle: Element = this.destinationGrid.getHeaderContent();
                        gridheaderEle.classList.add('e-grid-relative');
                        if (!isNullOrUndefined(this.destinationGrid.getContent())) {
                            const isHorizontalScrollAlone: boolean = this.destinationGrid.getContent().scrollWidth >
                                this.destinationGrid.getContent().clientWidth
                                && this.destinationGrid.getContent().scrollHeight <= this.destinationGrid.getContent().clientHeight;
                            div.style.width = isHorizontalScrollAlone ? (node as HTMLElement).offsetWidth + 'px' : (node as HTMLElement).offsetWidth - this.getScrollWidth() + 'px';
                        }
                        if (!gridheaderEle.querySelectorAll('.e-firstrow-dragborder').length) {
                            gridheaderEle.appendChild(div);
                        }
                    }
                }
                this.borderIndex = -1;
                if (this.destinationGrid.element.getElementsByClassName('e-lastrow-dragborder').length > 0) {
                    this.destinationGrid.element.getElementsByClassName('e-lastrow-dragborder')[0].remove();
                }
                else if (document.getElementsByClassName('e-lastrow-dragborder').length > 0) {
                    const lastRowBorder: HTMLElement = document.getElementsByClassName('e-lastrow-dragborder')[0] as HTMLElement;
                    if (!isNullOrUndefined(parentsUntil(lastRowBorder, 'e-grid'))) {
                        lastRowBorder.remove();
                    }
                }
            } else if (targetRow && (parseInt(startedRow.getAttribute('aria-rowindex'), 10) - 1 > targetRowIndex) || (this.parent.options.rowDropTarget && this.parent.element.id !== this.destinationGrid.element.id)) {
                if (this.parent.options.groupCount && this.parent.options.enableVirtualization) {
                    targetRowIndex = this.parent.getDataRows().indexOf(targetRow);
                }

                if (this.parent.options.groupCount && !targetRow.classList.contains('e-groupcaption')) {
                    element = targetRow;
                    if (!isNullOrUndefined(element) && !element.classList.contains('e-detailrow')) {
                        rowElement = [].slice.call(element.querySelectorAll('.e-rowcell,.e-rowdragdrop,.e-detailrowcollapse, .e-detailrowexpand'));
                    }
                }

                else {
                    if (targetRow === lastRow && mouseY > targetMidpoint) {
                        element = this.destinationGrid.getRowByIndex(targetRowIndex);
                    }
                    else {
                        element = this.destinationGrid.options.frozenRows ? this.destinationGrid.getRowByIndex(targetRowIndex)
                            : (targetRow.classList.contains('e-row')) ? this.destinationGrid.getRowByIndex(targetRow.rowIndex - 1) : element;
                    }
                    if (!isNullOrUndefined(element) && !element.classList.contains('e-detailrow')) {
                        rowElement = [].slice.call(element.querySelectorAll('.e-rowcell,.e-rowdragdrop,.e-detailrowcollapse, .e-detailrowexpand'));
                    }
                }

            } else {
                if (!isNullOrUndefined(element) && !element.classList.contains('e-detailrow') && !cloneElement.classList.contains('e-notallowedcur')) {
                    rowElement = [].slice.call(element.querySelectorAll('.e-rowcell,.e-rowdragdrop,.e-detailrowcollapse, .e-detailrowexpand'));
                }
            }
            if (rowElement.length > 0) {
                this.borderIndex = Number(parentsUntil(rowElement[0], 'e-row').getAttribute('aria-rowindex')) - 1;
                if (!isNullOrUndefined(parentsUntil(targetRow, 'e-editedrow'))) {
                    classList(cloneElement, ['e-notallowedcur'], ['e-defaultcur']);
                }
                if (!(this.parent.options.groupCount)) {
                    if (isNullOrUndefined(parentsUntil(targetRow, 'e-editedrow'))) {
                        addRemoveActiveClasses(rowElement, true, 'e-dragborder');
                    }
                    if (this.destinationGrid.element.getElementsByClassName('e-lastrow-dragborder').length > 0) {
                        this.destinationGrid.element.getElementsByClassName('e-lastrow-dragborder')[0].remove();
                    }
                    else if (document.getElementsByClassName('e-lastrow-dragborder').length > 0) {
                        const lastRowBorder: HTMLElement = document.getElementsByClassName('e-lastrow-dragborder')[0] as HTMLElement;
                        if (!isNullOrUndefined(parentsUntil(lastRowBorder, 'e-grid'))) {
                            lastRowBorder.remove();
                        }
                    }
                    if (this.destinationGrid.element.getElementsByClassName('e-firstrow-dragborder').length > 0) {
                        this.destinationGrid.element.getElementsByClassName('e-firstrow-dragborder')[0].remove();
                    } else if (document.getElementsByClassName('e-firstrow-dragborder').length > 0) {
                        const lastRowBorder: Element = document.getElementsByClassName('e-firstrow-dragborder')[0];
                        if (!isNullOrUndefined(parentsUntil(lastRowBorder, 'e-grid'))) {
                            lastRowBorder.remove();
                        }
                    }
                }
            }
        }
    }

    private addorRemoveDashedBorder(e: { target: HTMLElement, event: MouseEventArgs }, add: boolean,
                                    dataRowElements: HTMLElement[]): void {
        if (dataRowElements.length <= 0) {
            return;
        }
        const firstDataRow: HTMLElement = dataRowElements[0];
        const lastDataRow: HTMLElement = dataRowElements[dataRowElements.length - 1];
        let firstDataRowCells: HTMLElement[] = [];
        let lastDataRowCells: HTMLElement[] = [];
        firstDataRowCells = [].slice.call(firstDataRow.querySelectorAll('.e-rowcell:not(.e-hide),.e-rowdragdrop,.e-detailrowcollapse, .e-detailrowexpand'));
        lastDataRowCells = [].slice.call(lastDataRow.querySelectorAll('.e-rowcell:not(.e-hide),.e-rowdragdrop,.e-detailrowcollapse, .e-detailrowexpand'));
        addRemoveActiveClasses(firstDataRowCells, add, 'e-dragtop');
        addRemoveActiveClasses(lastDataRowCells, add, 'e-dragbottom');
        this.updateDragClasses(add, dataRowElements);
    }

    private updateDragClasses(add: boolean, dataRowElements: HTMLElement[]): void {
        for (let i: number = 0; i < dataRowElements.length; i++) {
            let rowElementCells: HTMLElement[] = [];
            rowElementCells = [].slice.call(dataRowElements[parseInt(i.toString(), 10)].querySelectorAll('.e-rowcell:not(.e-hide),.e-rowdragdrop,.e-detailrowcollapse, .e-detailrowexpand'));
            if (rowElementCells.length) {
                if (add) {
                    rowElementCells[0].classList.add('e-dragleft');
                    rowElementCells[rowElementCells.length - 1].classList.add('e-dragright');
                }
                else {
                    rowElementCells[0].classList.remove('e-dragleft');
                    rowElementCells[rowElementCells.length - 1].classList.remove('e-dragright');
                }
            }
        }
    }

    private getScrollWidth(): number {
        const scrollElem: HTMLElement = !isNullOrUndefined(this.destinationGrid) &&
            this.destinationGrid.element.id !== this.parent.element.id ?
            this.destinationGrid.getContent() : this.parent.getContent() as HTMLElement;
        return scrollElem.scrollWidth > scrollElem.offsetWidth ? getScrollBarWidth() : 0;
    }

    private removeFirstRowBorder(element: Element): void {
        if (this.destinationGrid.element.getElementsByClassName('e-firstrow-dragborder').length > 0 && element &&
            (element as HTMLTableRowElement).rowIndex !== 0) {
            this.destinationGrid.element.getElementsByClassName('e-firstrow-dragborder')[0].remove();
        }
        const cloneElement: Element = document.querySelector('.e-cloneproperties');
        const dropTarget: HTMLElement = document.getElementById(this.parent.options.rowDropTarget);
        if (isNullOrUndefined(element) && document.querySelectorAll('.e-firstrow-dragborder').length > 0 && !isNullOrUndefined(cloneElement) && cloneElement.classList.contains('e-notallowedcur') && !isNullOrUndefined(dropTarget)) {
            if (dropTarget.querySelectorAll('.e-firstrow-dragborder').length > 0) {
                dropTarget.querySelectorAll('.e-firstrow-dragborder')[0].remove();
            }
        }
    }

    private removeLastRowBorder(element: Element): void {
        const islastRowIndex: boolean = element &&
            !isNullOrUndefined(this.destinationGrid.getRowByIndex(this.destinationGrid.getRows().length - 1)) &&
            this.destinationGrid.getRowByIndex(this.destinationGrid.getRows().length - 1).getAttribute('data-uid') !==
            element.getAttribute('data-uid');
        if (this.destinationGrid.element.getElementsByClassName('e-lastrow-dragborder').length > 0 && element && islastRowIndex) {
            this.destinationGrid.element.getElementsByClassName('e-lastrow-dragborder')[0].remove();
        }
    }

    private removeBorder(element: Element): void {
        this.removeFirstRowBorder(element);
        this.removeLastRowBorder(element);
        const dropTarget: HTMLElement = document.getElementById(this.parent.options.rowDropTarget);
        const cloneElement: Element = document.querySelector('.e-cloneproperties');
        element = this.destinationGrid.getRows().filter((row: Element) =>
            row.querySelector('td.e-dragborder'))[0];
        if (element) {
            const rowElement: HTMLElement[] = [].slice.call(element.querySelectorAll('.e-dragborder'));
            addRemoveActiveClasses(rowElement, false, 'e-dragborder');
        }
        else if (isNullOrUndefined(element) && !isNullOrUndefined(dropTarget) && !isNullOrUndefined(cloneElement) && cloneElement.classList.contains('e-notallowedcur')) {
            const dragBorders: HTMLElement[] = [].slice.call(dropTarget.querySelectorAll('.e-dragborder'));
            addRemoveActiveClasses(dragBorders, false, 'e-dragborder');
        }
    }

    private getElementFromPosition(element: HTMLElement, event: MouseEventArgs): Element {
        const position: IPosition = getPosition(event);
        element.style.display = 'none';
        const target: Element = document.elementFromPoint(position.x, position.y);
        element.style.display = '';
        return target;
    }

    private getElementXPath(element: HTMLTableRowElement): string {
        if (!element) {
            return null;
        }
        if (element.id) {
            return `//[@id=${element.id}]` + (element.className !== '' ? ('.' + element.className.toLowerCase()) : '');
        } else if (element.tagName === 'BODY') {
            return '/html/body';
        } else {
            if (isNullOrUndefined(element) || isNullOrUndefined(element.parentElement) || isNullOrUndefined(element.parentElement.childNodes)) {
                return null;
            }
            const sameTagSiblings: HTMLElement[] = [].slice.call(element.parentElement.childNodes)
                .filter((e: HTMLTableRowElement) => e.nodeName === element.nodeName);
            const idx: number = sameTagSiblings.indexOf(element);

            return this.getElementXPath(element.parentNode as HTMLTableRowElement) +
                '/' +
                element.tagName.toLowerCase() + (element.className !== '' ? ('.' + element.className.toLowerCase()) : '') +
                (sameTagSiblings.length > 1 ? `[${idx + 1}]` : '');
        }
    }

    private getTargetIdx(targetRow: Element): number {
        return targetRow ? parseInt(targetRow.getAttribute('aria-rowindex'), 10) - 1 : 0;
    }

    private drop: Function = (e: DropEventArgs) => {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const droppedElement: HTMLElement = e.droppedElement || (e as any).helper;
        this.columnDrop({ target: e.target as HTMLTableRowElement, droppedElement: droppedElement, mouseEvent: e.event as MouseEvent });
        remove(droppedElement);
    }

    public columnDrop(e: { target: HTMLTableRowElement, droppedElement: HTMLElement, mouseEvent: MouseEvent }): void {
        let gObj: SfGrid = this.parent;
        let rowDragTargetId: string | null = null;
        const cloneElement: HTMLElement = document.querySelector('.e-cloneproperties') as HTMLElement;
        if (e.target.classList.contains('e-lastrow-dragborder') && !isNullOrUndefined(cloneElement) && !cloneElement.classList.contains('e-notallowedcur')) {
            const targetElementGrid: HTMLElement = parentsUntil(e.target, 'e-grid') as HTMLElement;
            if (!isNullOrUndefined(targetElementGrid)) {
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                gObj = (<any>targetElementGrid).blazor__instance;
            }
        }
        if (parentsUntil(gObj.element, 'e-detailrow') &&
            (isNullOrUndefined(this.cloneElementGrid) || (!isNullOrUndefined(e.droppedElement)
                && !isNullOrUndefined(e.droppedElement.parentElement) && e.droppedElement.parentElement.id !== gObj.element.id))
            && !isNullOrUndefined(gObj.options.rowDropTarget)) {
            this.cloneElementGrid = document.getElementById(gObj.options.rowDropTarget) as HTMLElement | null;
            if (!isNullOrUndefined(this.cloneElementGrid)) {
                rowDragTargetId = this.cloneElementGrid.id;
            }
        }
        const targetGridId: string = rowDragTargetId ? rowDragTargetId : e.droppedElement.parentElement.id;
        if (e.droppedElement.getAttribute('action') !== 'grouping') {
            const targetRow: HTMLTableRowElement = closestElement(e.target, 'tr') as HTMLTableRowElement;
            let srcControl: SfGrid;
            if ((e.droppedElement.querySelector('tr').getAttribute('single-dragrow') !== 'true' &&
                targetGridId === gObj.element.id)
                || (e.droppedElement.querySelector('tr').getAttribute('single-dragrow') === 'true' &&
                    targetGridId !== gObj.element.id)) {
                this.removeTargetGridBorder(this.parent);
                this.removeTargetGridBorder(this.destinationGrid);
                return;
            }
            if (targetGridId !== gObj.element.id) {
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                srcControl = this.cloneElementGrid && rowDragTargetId ?
                    (<any>this.cloneElementGrid).blazor__instance : (<any>e.droppedElement.parentElement).blazor__instance;
            }
            const dragStartRow: HTMLElement = srcControl.content.querySelector('.e-dragstartrow') as HTMLElement;
            if (srcControl.element.id !== gObj.element.id && srcControl.options.rowDropTarget !== gObj.element.id) {
                if (!isNullOrUndefined(dragStartRow)) {
                    dragStartRow.classList.remove('e-dragstartrow');
                    if (gObj.element.getElementsByClassName('e-lastrow-dragborder').length > 0) {
                        this.removeTargetGridBorder(gObj);
                    }
                }
                return;
            }
            let targetIndex: number = this.getTargetIdx(targetRow);
            const lastRow: HTMLElement = gObj.getContentTable().querySelector('tr:last-child') as HTMLElement;
            //const currentIndex = targetIndex;
            if (!isNullOrUndefined(targetRow)) {
                // Get the mouse position relative to the target row
                const mouseY: number = getPosition(e.mouseEvent).y - targetRow.getBoundingClientRect().top;
                // Calculate the midpoint of the target row
                const targetMidpoint: number = targetRow.offsetHeight / 2;
                if (e.target && !lastRow.classList.contains('e-emptyrow')) {
                    if (lastRow && targetRow === lastRow && mouseY > targetMidpoint) {
                        targetIndex = parseInt(lastRow.getAttribute('aria-rowindex'), 10);
                    }
                }
            }
            if (e.target && e.target.classList.contains('e-content') && gObj.options.allowEmptyAreaDrop) {
                if (lastRow) {
                    targetIndex = parseInt(lastRow.getAttribute('aria-rowindex'), 10);
                }
            }
            if (isNaN(targetIndex)) {
                targetIndex = 0;
            }
            if (gObj.options.allowPaging) {
                targetIndex = targetIndex + (gObj.options.currentPage * gObj.options.pageSize) - gObj.options.pageSize;
            }
            const targetClass: string = this.getElementXPath(e.target);
            const offsetParent: Element = parentsUntil(e.target, 'e-grid');
            const targetID: string = !isNullOrUndefined(offsetParent) ? offsetParent.id : '';
            const positions: ClientRect = e.target.getBoundingClientRect();
            this.removeTargetGridBorder(this.parent);
            let dragBetweenGrid: boolean = false;
            let isClonedRowNotSelected: boolean = false;
            const clonedRowElement: HTMLElement = parentsUntil(dragStartRow, 'e-row') as HTMLElement;
            if (!isNullOrUndefined(clonedRowElement)) {
                const ariaSelected: any = clonedRowElement.getAttribute('aria-selected');
                isClonedRowNotSelected = ariaSelected === 'false' || isNullOrUndefined(ariaSelected);
            }
            const fromIndex: number = isClonedRowNotSelected ? parseInt(clonedRowElement.getAttribute('aria-rowindex'), 10) - 1 : 0;
            if (gObj.options.tValue === srcControl.options.tValue) {

                if (!isNullOrUndefined(dragStartRow)) {
                    dragBetweenGrid = isClonedRowNotSelected;
                    dragStartRow.classList.remove('e-dragstartrow');
                }
                gObj.dotNetRef.invokeMethodAsync('ReorderRows', fromIndex, targetIndex, 'add', false, targetClass, targetID, positions, srcControl.dotNetRef, false, false, null, null, dragBetweenGrid, getPosition(e.mouseEvent).x, getPosition(e.mouseEvent).y);
                srcControl.dotNetRef.invokeMethodAsync('ReorderRows', fromIndex, targetIndex, 'delete', false, targetClass, targetID, positions, null, false, false, null, null, dragBetweenGrid, getPosition(e.mouseEvent).x, getPosition(e.mouseEvent).y);
            } else {
                if (!isNullOrUndefined(dragStartRow)) {
                    dragStartRow.classList.remove('e-dragstartrow');
                }
                srcControl.dotNetRef.invokeMethodAsync('ReorderRows', fromIndex, targetIndex, 'delete', false, targetClass, targetID, positions, null, false, true, null, null, dragBetweenGrid, getPosition(e.mouseEvent).x, getPosition(e.mouseEvent).y);
            }
        }
    }

    private removeTargetGridBorder(grid: SfGrid): void {
        if (!isNullOrUndefined(grid)) {
            if (grid.element.getElementsByClassName('e-firstrow-dragborder').length > 0) {
                grid.element.getElementsByClassName('e-firstrow-dragborder')[0].remove();
            }
            if (grid.element.getElementsByClassName('e-lastrow-dragborder').length > 0) {
                grid.element.getElementsByClassName('e-lastrow-dragborder')[0].remove();
            }
            else if (document.getElementsByClassName('e-lastrow-dragborder').length > 0) {
                const lastRowBorder: HTMLElement = document.getElementsByClassName('e-lastrow-dragborder')[0] as HTMLElement;
                if (!isNullOrUndefined(parentsUntil(lastRowBorder, 'e-grid'))) {
                    lastRowBorder.remove();
                }
            }
            removeClass(grid.element.querySelectorAll('.e-rowcell.e-dragborder,.e-detailrowcollapse.e-dragborder, .e-rowdragdrop.e-dragborder, e-detailrowexpand.e-dragborder'), ['e-dragborder']);
        }
    }

    public isTargetInEditMode(targetElement: Element): boolean {
        if (!isNullOrUndefined(parentsUntil(targetElement, 'e-editedrow')) || !isNullOrUndefined(parentsUntil(targetElement, 'e-editedbatchcell'))
            || !isNullOrUndefined(parentsUntil(targetElement, 'e-addedrow')) || !isNullOrUndefined(parentsUntil(targetElement, 'e-showAddNewRow'))) {
            return true;
        }
        return false;
    }

    /**
     * To destroy the print
     *
     * @returns {void}
     * @hidden
     */
    public destroy(): void {
        const gridElement: Element = this.parent.element;
        if (!gridElement || (!gridElement.querySelector('.e-gridheader') &&
            !gridElement.querySelector('.e-gridcontent'))) { return; }
        if (!isNullOrUndefined(this.draggable)) {
            this.draggable.destroy();
        }
        if (!isNullOrUndefined(this.droppable)) {
            this.droppable.destroy();
        }
    }

}
