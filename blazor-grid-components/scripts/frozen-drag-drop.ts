import { SfGrid } from './sf-grid-fn';
import { closest, removeClass, Browser, EventHandler, createElement, detach, addClass, isNullOrUndefined } from '@syncfusion/ej2-base';
import { parentsUntil, getScrollBarWidth } from './util';
import { FrozenLineOffsetPosition, Column } from './interfaces';

export const frozenDragClassList: FrozenDragClasses = {
    header: 'th.e-headercell',
    rowcell: '.e-rowcell',
    filterbarcell: '.e-filterbarcell',
    helper: 'e-frozen-helper',
    cursor: 'e-frozen-cursor',
    ariaColumnIndex: 'aria-colindex'
};

export interface FrozenDragClasses {
    helper: string;
    rowcell: string;
    filterbarcell: string;
    header: string;
    cursor: string;
    ariaColumnIndex: string;
}

/**
 *
 * Frozen column drag and drop handling.
 *
 * @hidden
 */
export class FrozenDD {

    private parent: SfGrid;
    private helperElement: HTMLElement;
    private currentCursorElement: HTMLElement;
    private currentBorderIndex: number = -1;
    private originalBorderIndex: number = -1;
    private borderCells: NodeListOf<Element>;
    private borderHeaderCell: HTMLElement;
    private filterBarCell: HTMLElement;
    private isLeftCursorDragging: boolean;
    private isLeftFixedCursorDragging: boolean;
    private isRightFixedCursorDragging: boolean;
    private visibleLtoROrderedColumns: Column[] = [];
    private visibleRtoLOrderedColumns: Column[] = [];
    private widthCollection: number[] = [];
    private visibleFrozenColumns: Column[] = [];
    private visibleFrozenLeftColumns: Column[] = [];
    //frozenDefaultBorderClass determine the border class to be added for the grid when there is no frozen columns
    private frozenDefaultBorderClass: string;
    private domRtoLOrderedColumns: Column[] = [];
    private domLtoROrderedColumns: Column[] = [];

    constructor(parent: SfGrid) {
        this.parent = parent;
        if (this.parent.options.allowFreezeLineMoving) {
            this.wireEvents();
        }
    }

    private wireEvents(): void {
        EventHandler.add(this.parent.getContent(), Browser.touchStartEvent, this.dragStart, this);
        EventHandler.add(this.parent.getHeaderContent(), Browser.touchStartEvent, this.dragStart, this);
    }

    public unwireEvents(): void {
        EventHandler.remove(this.parent.getContent(), Browser.touchStartEvent, this.dragStart);
        EventHandler.remove(this.parent.getHeaderContent(), Browser.touchStartEvent, this.dragStart);
    }

    private dragStart(e: PointerEvent | TouchEvent): void {
        if ((e.target as HTMLElement).classList.contains(frozenDragClassList.cursor)) {
            this.currentCursorElement = e.target as HTMLElement;
            this.parent.getContent().classList.add('e-freezeline-moving');
            this.isLeftCursorDragging = (this.currentCursorElement.classList.contains('e-frozen-left-cursor') || this.currentCursorElement.classList.contains('e-frozen-fixedleft-cursor') || this.currentCursorElement.classList.contains('e-frozen-fixedright-cursor')) && !this.currentCursorElement.classList.contains('e-frozen-default-cursor') || this.currentCursorElement.classList.contains('e-frozen-default-cursor') && this.currentCursorElement.classList.contains('e-frozen-right-cursor') ? true : false;
            this.isLeftFixedCursorDragging = this.currentCursorElement.classList.contains('e-frozen-fixedleft-cursor') ? true : false;
            this.isRightFixedCursorDragging = this.currentCursorElement.classList.contains('e-frozen-fixedright-cursor') ? true : false;
            this.appendHelper();
            let rowcell: HTMLElement = closest(this.currentCursorElement, frozenDragClassList.header) as HTMLElement;
            let uid: string;
            if (isNullOrUndefined(rowcell)) {
                rowcell = (closest(this.currentCursorElement, frozenDragClassList.rowcell) as HTMLElement);
                if (isNullOrUndefined(rowcell)) {
                    uid = closest(this.currentCursorElement, frozenDragClassList.filterbarcell).getAttribute('e-mappinguid');
                } else {
                    const colIndex: string = rowcell.getAttribute(frozenDragClassList.ariaColumnIndex);
                    const headerElement: HTMLElement = this.parent.getHeaderContent().querySelector('[' + frozenDragClassList.ariaColumnIndex + '="' + colIndex + '"]');
                    const mappingElement: HTMLElement = !isNullOrUndefined(headerElement) ? headerElement.querySelector('[e-mappinguid]') : null;
                    uid = !isNullOrUndefined(mappingElement) ? mappingElement.getAttribute('e-mappinguid') : '';
                }
            } else {
                const mappingElement: HTMLElement = rowcell.querySelector('[e-mappinguid]');
                uid = !isNullOrUndefined(mappingElement) ? mappingElement.getAttribute('e-mappinguid') : '';
            }
            const startIndex: number = this.parent.getColumns().filter((col: Column) => col.uid === uid)[0].index;
            this.setColumnsForBorderUpdate();
            EventHandler.add(document, Browser.touchEndEvent, this.dragEnd, this);
            this.parent.dotNetRef.invokeMethodAsync('InvokeFreezeLineMoving', {
                fromIndex: startIndex
            });
        }
    }

    private appendHelper(): void {
        this.helperElement = createElement('div', {
            className: frozenDragClassList.helper
        });
        this.setHelperHeight();
        this.parent.element.appendChild(this.helperElement);
    }

    private setHelperHeight(): void {
        let height: number = (<HTMLElement>this.parent.getContent()).offsetHeight;
        let headercell: HTMLElement = closest(this.currentCursorElement, frozenDragClassList.header) as HTMLElement;
        if (isNullOrUndefined(headercell)) {
            const rowcell: HTMLElement = closest(this.currentCursorElement, frozenDragClassList.rowcell) as HTMLElement;
            if (isNullOrUndefined(rowcell)) {
                const filterbarcell: HTMLElement = closest(this.currentCursorElement, frozenDragClassList.filterbarcell) as HTMLElement;
                const uid: string = filterbarcell.getAttribute('e-mappinguid');
                const headerContent: HTMLElement = this.parent.getHeaderContent() as HTMLElement;
                const mappingElement: HTMLElement = headerContent.querySelector('[e-mappinguid=' + uid + ']') as HTMLElement;
                if (!isNullOrUndefined(mappingElement) && !isNullOrUndefined(mappingElement.parentElement)) {
                    const parentHeaderElement: HTMLElement = mappingElement.parentElement;
                    headercell = parentHeaderElement.parentElement;
                }
            } else {
                const index: string = rowcell.getAttribute(frozenDragClassList.ariaColumnIndex);
                headercell = this.parent.getHeaderContent().querySelector('[' + frozenDragClassList.ariaColumnIndex + '="' + index + '"]');
            }
        }
        const tr: HTMLElement[] = [].slice.call(this.parent.getHeaderContent().querySelectorAll('tr'));
        for (let i: number = tr.indexOf(headercell.parentElement); i < tr.length; i++) {
            height += tr[parseInt(i.toString(), 10)].offsetHeight;
        }
        if (this.parent.getContent().scrollWidth > this.parent.getContent().clientWidth) {
            height -= 16;
        }
        const pos: FrozenLineOffsetPosition = this.calcPos(headercell);
        const top: number = this.calcPos(this.parent.getHeaderContent()).top;
        if (!this.currentCursorElement.classList.contains('e-frozen-default-cursor')) {
            if (headercell.classList.contains('e-rightfreeze')) {
                pos.left += (this.parent.options.enableRtl ? headercell.offsetWidth - 2 : 0 - 1);
            }
            else {
                pos.left += (this.parent.options.enableRtl ? 0 - 1 : headercell.offsetWidth - 2);
            }
        } else {
            pos.left = this.isLeftCursorDragging ? pos.left : (pos.left + headercell.offsetWidth - 2);
        }
        this.helperElement.style.cssText = 'height: ' + height + 'px; top: ' + top + 'px; left:' + Math.floor(pos.left) + 'px;';
    }

    private calcPos(elem: HTMLElement): FrozenLineOffsetPosition {
        let parentOffset: FrozenLineOffsetPosition = {
            top: 0,
            left: 0,
            right: 0
        };
        const offset: FrozenLineOffsetPosition = elem.getBoundingClientRect();
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
            left: offset.left - parentOffset.left,
            right: offset.right - parentOffset.right
        };
    }

    public preventFreezeLineMoving(isCancel: boolean): void {
        if (isCancel) {
            this.cancelFreezeLineMoving();
        } else {
            EventHandler.add(this.parent.element, Browser.touchMoveEvent, this.freezeLineDragging, this);
        }
    }

    private cancelFreezeLineMoving(): void {
        this.unwireEvents();
        detach(this.helperElement);
        this.setDefaultValue();
    }

    private setDefaultValue(): void {
        this.parent.getContent().classList.remove('e-freezeline-moving');
        this.addOrRemoveClasses('remove');
        this.helperElement = null;
        this.currentBorderIndex = -1;
        this.currentCursorElement = null;
        this.borderCells = null;
        this.widthCollection = [];
    }

    private freezeLineDragging(e: PointerEvent | TouchEvent): void {
        if (!this.helperElement) { return; }
        this.scrollHorizontally();
        this.updateHelper(e);
        this.updateBorder();
    }

    private setColumnsForBorderUpdate(): void {
        const gridColumns: Column[] = this.parent.getColumns();
        const frozenLeftColumns: Column[] = gridColumns.filter((col: Column) => col.freeze === 'Left' && col.isFrozen);
        this.visibleFrozenLeftColumns = frozenLeftColumns.filter((col: Column) => col.visible);
        const frozenRightColumns: Column[] = gridColumns.filter((col: Column) => col.freeze === 'Right' && col.isFrozen);
        const visibleFrozenRightColumns: Column[] = frozenRightColumns.filter((col: Column) => col.visible);
        if (this.parent.options.actualFrozenColumns > 0) {
            //Only executes if the Grid's FrozenColumns property is set
            const frozenColumns: Column[] = gridColumns.slice(0, this.parent.options.actualFrozenColumns);
            this.visibleFrozenColumns = frozenColumns.filter((col: Column) => col.visible);
            const movableColumns: Column[] = gridColumns.filter((col: Column) => (col.index > this.parent.options.actualFrozenColumns - 1) && (!col.isFrozen || (col.isFrozen && col.freeze === 'Fixed')));
            const visibleMovableColumns: Column[] = movableColumns.filter((col: Column) => col.visible);
            if (this.isLeftCursorDragging) {
                this.visibleLtoROrderedColumns = [].concat(this.visibleFrozenColumns,
                                                           this.visibleFrozenLeftColumns, visibleMovableColumns, visibleFrozenRightColumns);
                this.domLtoROrderedColumns = [].concat(frozenColumns, frozenLeftColumns, movableColumns, frozenRightColumns);
                this.widthCollection = this.pushColumnWidth(this.visibleLtoROrderedColumns, this.widthCollection);
            } else {
                this.visibleRtoLOrderedColumns = [].concat(visibleFrozenRightColumns.reverse(),
                                                           visibleMovableColumns.reverse(), this.visibleFrozenLeftColumns.reverse(),
                                                           this.visibleFrozenColumns.reverse());
                this.domRtoLOrderedColumns = [].concat(frozenRightColumns.reverse(),
                                                       movableColumns.reverse(), frozenLeftColumns.reverse(), frozenColumns.reverse());
                this.widthCollection = this.pushColumnWidth(this.visibleRtoLOrderedColumns, this.widthCollection);
            }
        } else {
            const movableColumns: Column[] = gridColumns.filter((col: Column) => !col.isFrozen || (col.isFrozen && col.freeze === 'Fixed'));
            const visibleMovableColumns: Column[] = movableColumns.filter((col: Column) => col.visible);
            if (this.isLeftCursorDragging) {
                this.domLtoROrderedColumns = [].concat(frozenLeftColumns, movableColumns, frozenRightColumns);
                this.visibleLtoROrderedColumns = [].concat(this.visibleFrozenLeftColumns, visibleMovableColumns, visibleFrozenRightColumns);
                this.widthCollection = this.pushColumnWidth(this.visibleLtoROrderedColumns, this.widthCollection);
            } else {
                this.domRtoLOrderedColumns = [].concat(frozenRightColumns.reverse(), movableColumns.reverse(), frozenLeftColumns.reverse());
                this.visibleRtoLOrderedColumns = [].concat(visibleFrozenRightColumns.reverse(),
                                                           visibleMovableColumns.reverse(), this.visibleFrozenLeftColumns.reverse());
                this.widthCollection = this.pushColumnWidth(this.visibleRtoLOrderedColumns, this.widthCollection);
            }
        }
    }

    private updateBorder(): void {
        let index: number;
        if (this.currentBorderIndex === -1) {
            if (!isNullOrUndefined(parentsUntil(this.currentCursorElement, 'e-rowcell'))) {
                this.currentBorderIndex = Number(parentsUntil(this.currentCursorElement, 'e-rowcell').getAttribute(frozenDragClassList.ariaColumnIndex));
            } else if (!isNullOrUndefined(parentsUntil(this.currentCursorElement, 'e-filterbarcell'))) {
                const uid: string = parentsUntil(this.currentCursorElement, 'e-filterbarcell').getAttribute('e-mappinguid');
                const headercell: HTMLElement = this.parent.getHeaderContent().querySelectorAll('[e-mappinguid=' + uid + ']')[0].parentElement;
                if (!isNullOrUndefined(headercell)) {
                    const gridHeaderCell: Element = headercell.parentElement;
                    this.currentBorderIndex = Number(gridHeaderCell.getAttribute(frozenDragClassList.ariaColumnIndex)) - 1;
                }
            } else {
                this.currentBorderIndex = Number(parentsUntil(this.currentCursorElement, 'e-headercell').getAttribute(frozenDragClassList.ariaColumnIndex)) - 1;
            }
        }
        const helperPosition: number = this.isLeftFixedCursorDragging || this.isRightFixedCursorDragging ?
            this.convertWidthToNumber(this.helperElement.style.left) + this.parent.getContent().scrollLeft : this.isLeftCursorDragging ?
                this.convertWidthToNumber(this.helperElement.style.left) : (-(this.calcPos(this.helperElement).right));
        if (helperPosition < this.widthCollection[0] / 2) {
            index = 0;
            this.frozenDefaultBorderClass = this.isLeftCursorDragging ? 'e-frozen-left-border' : 'e-frozen-right-border';
        } else if (helperPosition > this.widthCollection[0] / 2 && helperPosition < this.widthCollection[0]) {
            index = 0;
            this.frozenDefaultBorderClass = this.isLeftCursorDragging ? 'e-frozen-right-border' : 'e-frozen-left-border';
        } else {
            this.frozenDefaultBorderClass = 'default';
            for (let i: number = 0; (i < this.widthCollection.length && i + 1 !== this.widthCollection.length); i++) {
                const indexValue: number = parseInt(i.toString(), 10);
                if (helperPosition > this.widthCollection[parseInt(indexValue.toString(), 10)] && helperPosition <
                this.widthCollection[i + 1]) {
                    const colwidthBy2: number = (this.widthCollection[i + 1] - this.widthCollection[parseInt(indexValue.toString(), 10)])
                    / 2;
                    if (helperPosition < this.widthCollection[parseInt(indexValue.toString(), 10)] + colwidthBy2) {
                        index = i;
                    } else {
                        index = i + 1;
                    }
                    break;
                }
            }
        }
        if (!isNullOrUndefined(index) && this.currentBorderIndex !== index || (this.frozenDefaultBorderClass !== 'default' && index === this.currentBorderIndex)) {
            this.addOrRemoveClasses('remove');
            this.currentBorderIndex = index;
            const borderColumnUID: string = this.isLeftCursorDragging ?
                this.visibleLtoROrderedColumns[this.currentBorderIndex].uid : this.visibleRtoLOrderedColumns[this.currentBorderIndex].uid;
            const headerCells: NodeListOf<Element> = this.parent.getHeaderContent().querySelectorAll('[e-mappinguid="' + borderColumnUID + '"]');
            if (headerCells.length) {
                for (let i: number = 0; i < headerCells.length; i++) {
                    const headerCellElement: Element = headerCells[parseInt(i.toString(), 10)];
                    if (!isNullOrUndefined(headerCellElement) && !isNullOrUndefined(headerCellElement.parentElement) && headerCellElement.classList.contains('e-headercelldiv')) {
                        this.borderHeaderCell = headerCellElement.parentElement.parentElement;
                    }
                }
            }
            if (this.parent.getHeaderContent().querySelector('.e-filterbar')) {
                const filterBarCells: NodeListOf<HTMLElement> = this.parent.getHeaderContent().querySelectorAll('.e-filterbarcell');
                for (let i: number = 0; i < filterBarCells.length; i++) {
                    if (filterBarCells[parseInt(i.toString(), 10)].getAttribute('e-mappinguid') === borderColumnUID) {
                        this.filterBarCell = filterBarCells[parseInt(i.toString(), 10)];
                    }
                }
            }
            //currentBorderIndex and originalBorderIndex will vary if we drag the right frozen handler
            this.originalBorderIndex = Number(this.borderHeaderCell.getAttribute(frozenDragClassList.ariaColumnIndex)) - 1;
            this.borderCells = this.parent.getContent().querySelectorAll('[' + frozenDragClassList.ariaColumnIndex + '="' + (this.originalBorderIndex + 1) + '"]');
            this.addOrRemoveClasses('add');
        }
    }

    private addOrRemoveClasses(action: string): void {
        let classToAddOrRemove: string = this.isLeftCursorDragging ? 'e-frozen-right-border' : 'e-frozen-left-border';
        if (!isNullOrUndefined(this.borderCells)) {
            if (action === 'add') {
                classToAddOrRemove = (this.frozenDefaultBorderClass === 'default') ? classToAddOrRemove : this.frozenDefaultBorderClass;
                addClass(this.borderCells, classToAddOrRemove);
                this.borderHeaderCell.classList.add(classToAddOrRemove);
                if (!isNullOrUndefined(this.filterBarCell)) {
                    this.filterBarCell.classList.add(classToAddOrRemove);
                }
            } else {
                if (this.frozenDefaultBorderClass !== 'default') {
                    removeClass(this.borderCells, ['e-frozen-left-border', 'e-frozen-right-border']);
                    this.borderHeaderCell.classList.remove('e-frozen-right-border');
                    this.borderHeaderCell.classList.remove('e-frozen-left-border');
                    if (!isNullOrUndefined(this.filterBarCell)) {
                        this.filterBarCell.classList.remove('e-frozen-right-border');
                        this.filterBarCell.classList.remove('e-frozen-left-border');
                    }
                } else {
                    removeClass(this.borderCells, classToAddOrRemove);
                    this.borderHeaderCell.classList.remove(classToAddOrRemove);
                    if (!isNullOrUndefined(this.filterBarCell)) {
                        this.filterBarCell.classList.remove(classToAddOrRemove);
                    }
                }
            }
        }
    }

    private convertWidthToNumber(widthInString: string): number {
        const widthInNumber: number = Number(widthInString.toString().replace('px', ''));
        return widthInNumber;
    }

    private pushColumnWidth(columns: Column[], result: number[]): number[] {
        let previousWidth: number = 0;
        for (let i: number = 0; i < columns.length; i++) {
            const colWidth: number = this.convertWidthToNumber(columns[parseInt(i.toString(), 10)].width as string);
            result.push(previousWidth + colWidth);
            previousWidth = result[result.length - 1];
        }
        return result;
    }

    private scrollHorizontally(): void {
        if (this.parent.options.frozenColumns !== 0) {
            const movableContent: HTMLElement = this.parent.getContent();
            if (movableContent.scrollWidth > movableContent.clientWidth) {
                movableContent.scrollLeft = this.isLeftFixedCursorDragging || this.isRightFixedCursorDragging ?
                    movableContent.scrollLeft : this.isLeftCursorDragging ? 0 : movableContent.offsetWidth;
            }
        }
    }

    private updateHelper(e: PointerEvent | TouchEvent): void {
        const mouseX: number = this.getCurrentScreenX(e);
        const cursorLeft: number = this.calcPos(this.currentCursorElement).left;
        const gridLeft: number = this.parent.element.getBoundingClientRect().left;
        const mouseMove: number = mouseX - (gridLeft + cursorLeft);
        const gridOffSetWIdth: number = this.parent.getContent().offsetWidth;
        let newHelperLeft: number = cursorLeft + mouseMove;
        let movableElement: HTMLElement = this.parent.getContent().querySelector('.e-movablecontent');
        if (!movableElement) {
            movableElement = this.parent.getContent();
        }
        if (this.isLeftCursorDragging && (!this.isLeftFixedCursorDragging && !this.isRightFixedCursorDragging)) {
            const movableRight: number = this.calcPos(movableElement).right;
            newHelperLeft = newHelperLeft > (gridOffSetWIdth + movableRight) ? (gridOffSetWIdth + movableRight - 1) : newHelperLeft;
        } else if (!this.isLeftFixedCursorDragging && !this.isRightFixedCursorDragging) {
            const movableLeft: number = this.calcPos(movableElement).left;
            newHelperLeft = newHelperLeft < movableLeft ? (movableLeft + 1) : newHelperLeft;
            newHelperLeft = newHelperLeft > (gridOffSetWIdth - getScrollBarWidth()) ?
                (gridOffSetWIdth - getScrollBarWidth() - 1) : newHelperLeft;
        }
        this.helperElement.style.left = newHelperLeft + 'px';
        this.helperElement.style.zIndex = '10';
    }

    private getCurrentScreenX(e: PointerEvent | TouchEvent): number {
        if ((e as TouchEvent).touches && (e as TouchEvent).touches.length) {
            return (e as TouchEvent).touches[0].clientX;
        } else {
            return (e as PointerEvent).clientX;
        }
    }

    private dragEnd(): void {
        if (!this.helperElement) { return; }
        EventHandler.remove(this.parent.element, Browser.touchMoveEvent, this.freezeLineDragging);
        EventHandler.remove(document, Browser.touchEndEvent, this.dragEnd);
        const params: {
            startIndex: number,
            endIndex: number,
            freezeDirection: string,
            isFrozen: boolean,
            freezeLineMovingDirection: string,
            frozenColumnsUidCollection: string[]
        } = this.getServerParams();
        let frozenColumns: number = this.parent.options.actualFrozenColumns;
        if (this.parent.options.actualFrozenColumns > 0 && !params.isFrozen) {
            for (let i: number = 0; i < params.frozenColumnsUidCollection.length; i++) {
                for (let j: number = 0; j < this.visibleFrozenColumns.length; j++) {
                    if (this.visibleFrozenColumns[parseInt(j.toString(), 10)].uid ===
                    params.frozenColumnsUidCollection[parseInt(i.toString(), 10)]) {
                        frozenColumns--;
                    }
                }
            }
        }
        const hasGridStructureChanges: boolean = (this.currentCursorElement.classList.contains('e-frozen-default-cursor') ||
            (this.isLeftCursorDragging && this.frozenDefaultBorderClass === 'e-frozen-left-border') ||
            (!this.isLeftCursorDragging && this.frozenDefaultBorderClass === 'e-frozen-right-border')) ? true : false;
        detach(this.helperElement);
        this.setDefaultValue();
        if (!this.checkForServerCall(
            params.isFrozen,
            params.frozenColumnsUidCollection,
            params.endIndex,
            params.freezeDirection,
            params.freezeLineMovingDirection
        )) {
            return;
        }
        this.parent.dotNetRef.invokeMethodAsync('InvokeFreezeLineMoved', {
            frozenColumnsUidCollection: params.frozenColumnsUidCollection,
            freezeDirection: params.freezeDirection,
            freezeLineMovingDirection: params.freezeLineMovingDirection,
            isFrozen: params.isFrozen,
            fromIndex: params.startIndex,
            toIndex: params.endIndex,
            frozenColumnsCount: frozenColumns,
            hasGridStructureChanges: hasGridStructureChanges
        });
    }

    private getServerParams(): {
        startIndex: number, endIndex: number, freezeDirection: string,
        isFrozen: boolean, freezeLineMovingDirection: string, frozenColumnsUidCollection: string[]
    } {
        let freezeDirection: string;
        let isFrozen: boolean;
        const frozenColumnsUidCollection: string[] = [];
        let freezeLineMovingDirection: string;
        let startIndex: number = -1;
        let endIndex: number = -1;
        if (!isNullOrUndefined(this.borderCells)) {
            const helperLeft: number = this.convertWidthToNumber(this.helperElement.style.left);
            const cursorLeft: number = this.calcPos(this.currentCursorElement).left;
            let rowcell: HTMLElement = closest(this.currentCursorElement, frozenDragClassList.rowcell) as HTMLElement;
            if (isNullOrUndefined(rowcell)) {
                rowcell = closest(this.currentCursorElement, frozenDragClassList.header) as HTMLElement;
                if (isNullOrUndefined(rowcell)) {
                    const uid: string = parentsUntil(this.currentCursorElement, 'e-filterbarcell').getAttribute('e-mappinguid');
                    rowcell = (this.parent.getHeaderContent().querySelectorAll('[e-mappinguid="' + uid + '"]')[0]
                        .parentElement as HTMLElement).parentElement;
                }
            }
            startIndex = Number(rowcell.getAttribute(frozenDragClassList.ariaColumnIndex)) - 1;
            endIndex = Number(this.borderCells[0].getAttribute(frozenDragClassList.ariaColumnIndex)) - 1;
            if (this.isLeftCursorDragging) {
                freezeDirection = 'Left';
                if (this.isLeftFixedCursorDragging || this.isRightFixedCursorDragging) {
                    freezeDirection = 'Fixed';
                }
                if (helperLeft > cursorLeft) {
                    //Frozen Left Handler moving towards right side to add frozen columns
                    if (this.isLeftFixedCursorDragging) {
                        isFrozen = false;
                    }
                    else {
                        isFrozen = true;
                    }
                    freezeLineMovingDirection = 'Right';
                    const iterationStartIndex: number = (this.parent.options.frozenColumns === 0 ||
                        (this.parent.options.actualFrozenColumns === 0 && this.visibleFrozenLeftColumns.length === 0)) ? 0 :
                        (startIndex + 1);
                    const iterationEndIndex: number = (this.currentCursorElement.classList.contains('e-frozen-default-cursor')) ? (this.frozenDefaultBorderClass === 'e-frozen-left-border' ? -1 : endIndex) : endIndex;
                    for (let i: number = iterationStartIndex; i <= iterationEndIndex; i++) {
                        if (this.domLtoROrderedColumns[parseInt(i.toString(), 10)].visible) {
                            frozenColumnsUidCollection.push(this.domLtoROrderedColumns[parseInt(i.toString(), 10)].uid);
                        }
                    }
                } else {
                    //Frozen Left Handler moving towards left side to remove frozen columns
                    if (this.isLeftFixedCursorDragging) {
                        isFrozen = true;
                    }
                    else {
                        isFrozen = false;
                    }
                    freezeLineMovingDirection = 'Left';
                    if (this.frozenDefaultBorderClass !== 'e-frozen-left-border') {
                        for (let i: number = startIndex; i > endIndex; i--) {
                            if (this.domLtoROrderedColumns[parseInt(i.toString(), 10)].visible) {
                                frozenColumnsUidCollection.push(this.domLtoROrderedColumns[parseInt(i.toString(), 10)].uid);
                            }
                        }
                    } else {
                        //Frozen Left Handler moving towards left side to remove all the frozen columns
                        for (let i: number = startIndex; i !== -1; i--) {
                            if (this.domLtoROrderedColumns[parseInt(i.toString(), 10)].visible) {
                                frozenColumnsUidCollection.push(this.domLtoROrderedColumns[parseInt(i.toString(), 10)].uid);
                            }
                        }
                    }
                }
            } else {
                const reverseArray: Column[] = this.domRtoLOrderedColumns.reverse();
                if (helperLeft < cursorLeft) {
                    //Frozen Right Handler moving towards left side to add frozen columns
                    isFrozen = true;
                    freezeDirection = 'Right';
                    freezeLineMovingDirection = 'Left';
                    if (this.frozenDefaultBorderClass !== 'e-frozen-right-border') {
                        startIndex = this.currentCursorElement.classList.contains('e-frozen-default-cursor') ? startIndex + 1 : startIndex;
                        for (let i: number = endIndex; i < startIndex; i++) {
                            if (reverseArray[parseInt(i.toString(), 10)].visible) {
                                frozenColumnsUidCollection.push(reverseArray[parseInt(i.toString(), 10)].uid);
                            }
                        }
                    }
                } else {
                    //Frozen Right Handler moving towards right side to remove frozen columns
                    isFrozen = false;
                    freezeDirection = 'Left';
                    freezeLineMovingDirection = 'Right';
                    if (this.frozenDefaultBorderClass === 'e-frozen-right-border') {
                        //Frozen Right Handler moving towards right side to remove all the frozen columns
                        for (let i: number = startIndex; i <= endIndex; i++) {
                            if (reverseArray[parseInt(i.toString(), 10)].visible) {
                                frozenColumnsUidCollection.push(reverseArray[parseInt(i.toString(), 10)].uid);
                            }
                        }
                    } else {
                        for (let i: number = startIndex; i < endIndex; i++) {
                            if (reverseArray[parseInt(i.toString(), 10)].visible) {
                                frozenColumnsUidCollection.push(reverseArray[parseInt(i.toString(), 10)].uid);
                            }
                        }
                    }
                }
            }
        }
        return {
            startIndex: startIndex, endIndex: endIndex, freezeDirection: freezeDirection, isFrozen: isFrozen,
            freezeLineMovingDirection: freezeLineMovingDirection, frozenColumnsUidCollection: frozenColumnsUidCollection
        };
    }

    private checkForServerCall(isFrozen: boolean, frozenColumnsUidCollection: string[], endIndex: number, freezeDirection: string,
                               freezeLineMovingDirection: string): boolean {
        let allowServerCall: boolean = true;
        let originalFrozenColumns: Column[] = this.parent.columnModel.slice(0, this.parent.options.actualFrozenColumns);
        originalFrozenColumns = originalFrozenColumns.filter((col: Column) => col.visible);
        originalFrozenColumns = originalFrozenColumns.concat(this.parent.columnModel.filter((col: Column) => col.isFrozen && col.visible));
        if (isFrozen) {
            for (let i: number = 0; i < this.parent.columnModel.length; i++) {
                for (let j: number = 0; j < frozenColumnsUidCollection.length; j++) {
                    if (frozenColumnsUidCollection[parseInt(j.toString(), 10)] ===
                        this.parent.columnModel[parseInt(i.toString(), 10)].uid) {
                        let addColumn: boolean = true;
                        for (let k: number = 0; k < originalFrozenColumns.length; k++) {
                            if (originalFrozenColumns[parseInt(k.toString(), 10)].uid ===
                            frozenColumnsUidCollection[parseInt(j.toString(), 10)]) {
                                addColumn = false;
                            }
                        }
                        if (addColumn) {
                            originalFrozenColumns.push(this.parent.columnModel[parseInt(i.toString(), 10)]);
                        }
                    }
                }
            }
        }
        let totalFrozenWidth: number = 0;
        for (let i: number = 0; i < originalFrozenColumns.length; i++) {
            totalFrozenWidth += +(originalFrozenColumns[parseInt(i.toString(), 10)].width);
        }
        if (this.parent.element.offsetWidth <= totalFrozenWidth) {
            allowServerCall = false;
        }
        if (!isNullOrUndefined(this.parent.getContent().querySelector('.e-movablecontent'))) {
            const movableRow: HTMLCollectionOf<HTMLTableCellElement> = this.parent.getMovableDataRows()[0].getElementsByTagName('td');
            const frozenRow: HTMLCollectionOf<HTMLTableCellElement> = this.parent.getFrozenDataRows()[0].getElementsByTagName('td');
            const targetIndex: string = endIndex.toString();
            if ((freezeLineMovingDirection === 'Right' && (parseInt(movableRow[movableRow.length - 1].getAttribute('aria-colindex'), 10) - 1).toString() === targetIndex) ||
                (freezeLineMovingDirection === 'Left' && (parseInt(movableRow[0].getAttribute('aria-colindex'), 10) - 1).toString() === targetIndex) ||
                ((parseInt(frozenRow[0].getAttribute('aria-colindex'), 10) - 1).toString() === targetIndex && freezeDirection === 'Right' && freezeLineMovingDirection === 'Left')) {
                allowServerCall = false;
            }
        }
        return allowServerCall;
    }
}
