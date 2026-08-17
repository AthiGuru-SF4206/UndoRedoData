import { isNullOrUndefined } from '@syncfusion/ej2-base';
import { SfGrid } from './sf-grid-fn';
import { BlazorGridElement, Column } from './interfaces';
import { getScrollBarWidth } from './util';
/**
 * Frozen rows and column handling
 */
export class Freeze {
    private frozenHeader: HTMLElement;
    private movableHeader: HTMLElement;
    private element: BlazorGridElement;
    private parent: SfGrid;
    constructor(parent?: SfGrid) {
        this.parent = parent;
    }

    public refreshFreeze(obj: { case: string, isModeChg?: boolean }): void {
        if (obj.case === 'textwrap' || obj.case === 'refreshHeight') {
            let fRows: NodeListOf<HTMLElement>;
            let mRows: NodeListOf<HTMLElement>;
            let frRows: NodeListOf<HTMLElement>;
            const fHdr: Element = this.getFrozenHeader();
            const mHdr: Element = this.getMovableHeader();
            const cont: Element = this.parent.getContent();
            const wrapMode: string = this.parent.options.wrapMode;
            if (isNullOrUndefined(fHdr) && isNullOrUndefined(mHdr)) {
                return;
            }
            if (obj.case === 'textwrap') {
                if (wrapMode === 'Both' || obj.isModeChg) {
                    fRows = fHdr.querySelectorAll('tr') as NodeListOf<HTMLElement>;
                    mRows = mHdr.querySelectorAll('tr') as NodeListOf<HTMLElement>;
                    if (this.parent.options.frozenName === 'LeftRight') {
                        frRows = this.parent.element.querySelectorAll('.e-frozenheader')[1].querySelectorAll('tr') as NodeListOf<HTMLElement>;
                    }
                } else {
                    fRows = fHdr.querySelector(wrapMode === 'Content' ?
                        'tbody' : 'thead').querySelectorAll('tr') as NodeListOf<HTMLElement>;
                    mRows = mHdr.querySelector(wrapMode === 'Content' ?
                        'tbody' : 'thead').querySelectorAll('tr') as NodeListOf<HTMLElement>;
                }
                if (!this.parent.getHeaderContent().querySelectorAll('.e-stackedheadercell').length) {
                    this.setWrapHeight(fRows, mRows, obj.isModeChg, false, false, frRows);
                }
                this.refreshStackedHdrHgt();
            } else if (obj.case === 'refreshHeight') {
                if (this.parent.options.frozenName === 'LeftRight') {
                    frRows = cont.querySelector('.e-frozen-right-content').querySelectorAll('tr') as NodeListOf<HTMLElement>;
                }
                this.setWrapHeight(
                    cont.querySelector('.e-frozencontent').querySelectorAll('tr'),
                    cont.querySelector('.e-movablecontent').querySelectorAll('tr'), obj.isModeChg, false, false, frRows);
                if (!this.parent.getHeaderContent().querySelectorAll('.e-stackedheadercell').length) {
                    if (this.parent.options.frozenName === 'LeftRight') {
                        frRows = this.parent.element.querySelectorAll('.e-frozenheader')[1].querySelectorAll('tr') as NodeListOf<HTMLElement>;
                    }
                    this.setWrapHeight(fHdr.querySelectorAll('tr'), mHdr.querySelectorAll('tr'), obj.isModeChg, false, false, frRows);
                }
            }
        }
    }

    public updateResizeHandler(): void {
        const elements: HTMLElement[] = [].slice.call(this.parent.getHeaderContent().querySelectorAll('.e-rhandler'));
        for (let i: number = 0; i < elements.length; i++) {
            const headerCellContainer: HTMLElement = elements[parseInt(i.toString(), 10)].parentElement as HTMLElement;
            if (!isNullOrUndefined(headerCellContainer) && !isNullOrUndefined(headerCellContainer.parentElement)) {
                elements[parseInt(i.toString(), 10)].style.height =
                    (headerCellContainer.parentElement as HTMLElement).offsetHeight + 'px';
            }
        }
    }

    private setWrapHeight(
        fRows: NodeListOf<HTMLElement>, mRows: NodeListOf<HTMLElement>, isModeChg: boolean,
        isContReset?: boolean, isStackedHdr?: boolean, frRows?: NodeListOf<HTMLElement>): void {
        let fRowHgt: number;
        let mRowHgt: number;
        let frRowHgt: number;
        let maxHeight: number;
        const isWrap: boolean = this.parent.options.allowTextWrap;
        const wrapMode: string = this.parent.options.wrapMode;
        const tHead: Element = this.parent.getHeaderContent().querySelector('thead');
        const tBody: Element = this.parent.getHeaderContent().querySelector('tbody');
        const height: number[] = [];
        const width: number[] = [];
        const rightHeight: number[] = [];
        for (let i: number = 0, len: number = fRows.length; i < len; i++) { //separate loop for performance issue
            if (!isNullOrUndefined(fRows[parseInt(i.toString(), 10)]) && !isNullOrUndefined(mRows[parseInt(i.toString(), 10)])) {
                if (frRows) {
                    rightHeight[parseInt(i.toString(), 10)] = frRows[parseInt(i.toString(), 10)].getBoundingClientRect().height;
                }
                height[parseInt(i.toString(), 10)] = fRows[parseInt(i.toString(), 10)].getBoundingClientRect().height; //https://pagebuildersandwich.com/increased-plugins-performance-200/
                width[parseInt(i.toString(), 10)] = mRows[parseInt(i.toString(), 10)].getBoundingClientRect().height;
            }
        }
        for (let i: number = 0, len: number = fRows.length; i < len; i++) {
            if (isModeChg && ((wrapMode === 'Header' && isContReset) || ((wrapMode === 'Content' && tHead.contains(fRows[parseInt(i.toString(), 10)]))
                || (wrapMode === 'Header' && tBody.contains(fRows[parseInt(i.toString(), 10)])))) || isStackedHdr) {
                if (frRows[parseInt(i.toString(), 10)]) { frRows[parseInt(i.toString(), 10)].style.height = null; }
                fRows[parseInt(i.toString(), 10)].style.height = null;
                mRows[parseInt(i.toString(), 10)].style.height = null;
            }
            fRowHgt = height[parseInt(i.toString(), 10)];
            mRowHgt = width[parseInt(i.toString(), 10)];
            frRowHgt = rightHeight[parseInt(i.toString(), 10)] ? rightHeight[parseInt(i.toString(), 10)] : 0;
            if (this.parent.options.rowHeight !== 0) {
                maxHeight = this.parent.options.rowHeight;
            }
            else{
                maxHeight = Math.max(fRowHgt, mRowHgt, frRowHgt);
                mRows[parseInt(i.toString(), 10)].style.height = maxHeight + 'px';
                fRows[parseInt(i.toString(), 10)].style.height = maxHeight + 'px';
            }
            if (frRows) {
                frRows[parseInt(i.toString(), 10)].style.height = maxHeight + 'px';
            }
            //TODO: check below commented code is not working hence used above
            // if (!isNullOrUndefined(fRows[i]) && fRows[i].childElementCount && ((isWrap && fRowHgt < mRowHgt) ||
            //     (!isWrap && fRowHgt < mRowHgt))) {p
            //     fRows[i].style.height = mRowHgt + 'px';
            // }
            // if (mRows && !isNullOrUndefined(mRows[i]) && mRows[i].childElementCount && ((isWrap && fRowHgt > mRowHgt) ||
            //     (!isWrap && fRowHgt > mRowHgt))) {
            //     mRows[i].style.height = fRowHgt + 'px';
            // }
        }
        if (isWrap) {
            this.setFrozenHeight();
        }
    }

    public setFrozenHeight(minWidth?: number): void {

        if (this.parent.options.isPreventScrollEvent) {
            return;
        }
        const movableContent: HTMLElement = this.parent.element.querySelector('.e-content') as HTMLElement;
        const frozenscrollbarX: NodeListOf<HTMLElement>  = this.parent.element.querySelectorAll('.e-frozenscrollbar') as NodeListOf<HTMLElement>;
        const movablescrollbarX: HTMLElement = this.parent.element.querySelector('.e-movablescrollbar') as HTMLElement;
        const movableChildScrollBarX: HTMLElement = this.parent.element.querySelector('.e-movablechild') as HTMLElement;
        const content: HTMLElement = this.parent.element.querySelector('.e-content') as HTMLElement;
        if (this.parent.options.frozenColumns > 0 && this.parent.options.height === '100%' && this.parent.options.enableColumnVirtualization) {
            const scrollBar: HTMLElement = this.parent.element.querySelector('.e-movablescrollbar') as HTMLElement;
            const scrollBarHeight: number = scrollBar.offsetHeight;
            content.style.height = 'calc(100% - ' + scrollBarHeight + 'px)';
        }
        if (!isNullOrUndefined(movableContent))
        {
            const columns: HTMLElement[] = Array.from(
                (movableContent.getElementsByClassName('e-table')[0] as HTMLElement)
                    .querySelectorAll('col') as NodeListOf<HTMLElement>
            );
            const autoWidthColumns: HTMLElement[] = columns.filter((col: HTMLElement) => col.getAttribute('style') === 'width: auto');
            minWidth = autoWidthColumns.length > 0 && minWidth ? minWidth : 0;
        }
        if (movablescrollbarX && this.parent.options.height !== '100%' && this.parent.options.height !== 'auto') {
            const parentHeight: number = parseInt((this.parent.options.height as string).split ? (this.parent.options.height as string).split('px')[0] : this.parent.options.height, 10);
            const contentHeight: number = parentHeight - movablescrollbarX.offsetHeight;
            content.style.height = contentHeight + 'px';
        }
        if (((this.parent.options.enableVirtualization && this.parent.options.enableColumnVirtualization) ||
            this.parent.options.enableColumnVirtualization) && frozenscrollbarX) {
            const columns: Column[] = this.parent.options.columns;
            let totalColumnWidth: number = 0;
            for (let i: number = 0; i < columns.length; i++) {
                if (columns[parseInt(i.toString(), 10)].visible && !isNullOrUndefined(columns[parseInt(i.toString(), 10)].width)) {
                    totalColumnWidth += parseFloat((columns[parseInt(i.toString(), 10)].width as string).split ? (columns[parseInt(i.toString(), 10)].width as string).split('px')[0] : columns[parseInt(i.toString(), 10)].width.toString());
                }
            }
            movableChildScrollBarX.style.width = totalColumnWidth +  getScrollBarWidth() + 'px';
        } else {
            if (!isNullOrUndefined(movablescrollbarX) && !isNullOrUndefined(movableChildScrollBarX)) {
                movablescrollbarX.style.width = movableContent.offsetWidth + 'px';
                movableChildScrollBarX.style.width = (movableContent.getElementsByClassName('e-table')[0] as HTMLElement).offsetWidth + minWidth + getScrollBarWidth() + 'px';
            }
        }
        if (!isNullOrUndefined(movableContent)) {
            this.parent.scrollModule.setPadding();
        }
        //if (movableContent.scrollWidth - movableContent.clientWidth) {
        //TODO: why we need commented code?
        // frozenContent.style.height = movableContentHeight - height + 'px';
        // frozenContent.style.borderBottom = '';
        // } else {
        //     frozenContent.style.height = movableContentHeight + 'px';
        //     if ((frozenContent.scrollHeight <= frozenContent.clientHeight) ||
        //         (movableContent.scrollHeight <= movableContent.clientHeight)) {
        //         this.parent.scrollModule.removePadding();
        //     }
        //     frozenContent.style.borderBottom = '0px';
        // }
    }

    private updateStackedFrozenHeight ( fTr: NodeListOf<Element>, mTr: NodeListOf<Element>): void {
        let fRowSpan: { min: number, max: number };
        let mRowSpan: { min: number, max: number };
        for (let i: number = 0, len: number = fTr.length; i < len; i++) {
            fRowSpan = this.getRowSpan(fTr[parseInt(i.toString(), 10)]);
            mRowSpan = this.getRowSpan(mTr[parseInt(i.toString(), 10)]);

            if (fRowSpan.min > 1) {
                this.updateStackedHdrRowHgt(i, fRowSpan.max, fTr[parseInt(i.toString(), 10)], mTr);
            } else if (mRowSpan.min > 1) {
                this.updateStackedHdrRowHgt(i, mRowSpan.max, mTr[parseInt(i.toString(), 10)], fTr);
            }
        }
    }

    public refreshStackedHdrHgt(): void {
        let fRTr: NodeListOf<Element>;
        let fRTrL: NodeListOf<Element>;
        let maxLenCol: NodeListOf<Element>;
        const fTr: NodeListOf<Element> = this.getFrozenHeader().querySelectorAll('.e-columnheader');
        const mTr: NodeListOf<Element> = this.getMovableHeader().querySelectorAll('.e-columnheader');
        if (this.parent.options.frozenName === 'LeftRight') {
            fRTrL = this.parent.element.querySelectorAll('.e-frozenheader')[0].querySelectorAll('.e-columnheader');
            fRTr = this.parent.element.querySelectorAll('.e-frozenheader')[1].querySelectorAll('.e-columnheader');
            maxLenCol = (mTr.length > fRTrL.length) ? ((mTr.length > fRTr.length) ? mTr : fRTr)
                : ((fRTrL.length > fRTr.length) ? fRTrL : fRTr);
            if (maxLenCol === fRTr) {
                this.updateStackedFrozenHeight(fRTrL, maxLenCol);
                this.updateStackedFrozenHeight(fRTr, mTr);
            }
            else if (maxLenCol === fRTrL) {
                this.updateStackedFrozenHeight(fRTrL, mTr);
                this.updateStackedFrozenHeight(fRTr, maxLenCol);
            }
            else {
                this.updateStackedFrozenHeight(fRTrL, maxLenCol);
                this.updateStackedFrozenHeight(fRTr, mTr);
            }
        }
        else {
            this.updateStackedFrozenHeight(fTr, mTr);
        }
        if (this.parent.options.allowResizing) {
            this.updateResizeHandler();
        }
    }

    private getRowSpan(row: Element): { min: number, max: number } {
        let rSpan: number;
        let minRowSpan: number;
        let maxRowSpan: number;
        for (let i: number = 0, len: number = row.childElementCount; i < len; i++) {
            if (i === 0) {
                minRowSpan = (row.children[0] as HTMLTableDataCellElement).rowSpan;
            }
            rSpan = (row.children[parseInt(i.toString(), 10)] as HTMLTableDataCellElement).rowSpan;
            minRowSpan = Math.min(rSpan, minRowSpan);
            maxRowSpan = Math.max(rSpan, minRowSpan);
        }
        return { min: minRowSpan, max: maxRowSpan };
    }

    private updateStackedHdrRowHgt(idx: number, maxRowSpan: number, row: Element, rows: NodeListOf<Element>): void {
        let height: number = 0;
        for (let i: number = 0; i < maxRowSpan; i++) {
            height += (rows[idx + i] as HTMLElement).style.height ?
                parseInt((rows[idx + i] as HTMLElement).style.height, 10) : (rows[idx + i] as HTMLElement).offsetHeight;
        }
        (row as HTMLElement).style.height = height + 'px';
    }

    public getFrozenHeader(): Element {
        return this.frozenHeader;
    }

    public getMovableHeader(): Element {
        return this.movableHeader;
    }

    public refreshRowHeight(): void {
        if (this.parent.options.rowHeight !== 0) { return; }
    }

    public clearWrapHeight(): void {
        const fn: Function = (fRows: NodeListOf<HTMLElement>, mRows: NodeListOf<HTMLElement>) => {
            for (let i: number = 0, len: number = fRows.length; i < len; i++) {
                if (!isNullOrUndefined(fRows[parseInt(i.toString(), 10)]) && !isNullOrUndefined(mRows[parseInt(i.toString(), 10)])) {
                    fRows[parseInt(i.toString(), 10)].style.height = null;
                    mRows[parseInt(i.toString(), 10)].style.height = null;
                }
            }
        };
        let fRows: NodeListOf<HTMLElement>; let mRows: NodeListOf<HTMLElement>;
        if (this.parent.options.frozenColumns) {
            if (this.parent.options.frozenRows || this.parent.options.wrapMode === 'Both' || this.parent.options.wrapMode === 'Header') {
                fRows = this.parent.element.querySelector('.e-frozenheader').querySelectorAll('tr');
                mRows = this.parent.element.querySelector('.e-movableheader').querySelectorAll('tr');
                fn(fRows, mRows);
            }
            fRows = this.parent.element.querySelector('.e-frozencontent').querySelectorAll('tr');
            mRows = this.parent.element.querySelector('.e-movablecontent').querySelectorAll('tr');
            fn(fRows, mRows);
        }

        if (this.parent.options.frozenRows && this.parent.options.frozenColumns === 0) {
            fRows = this.parent.element.querySelector('.e-headercontent').querySelectorAll('tr');
            mRows = this.parent.element.querySelector('.e-content').querySelectorAll('tr');
            fn(fRows, mRows);
        }
    }

    public updateFrozenColumnStyles(): void {
        this.updateLeftFreezePosition(this.parent.getHeaderContent().querySelectorAll('tr'), 'th');
        this.updateLeftFreezePosition(Array.from(this.parent.getDataRows()), 'td');
    }

    private updateLeftFreezePosition(rows: NodeListOf<HTMLTableRowElement> | Element[], cellSelector: string): void {
        for (let i: number = 0; i < rows.length; i++) {
            const cells: NodeListOf<HTMLTableCellElement> = rows[parseInt(i.toString(), 10)].querySelectorAll(cellSelector);
            for (let j: number = 0; j < cells.length; j++) {
                if (cells[parseInt(j.toString(), 10)].classList.contains('e-leftfreeze')) {
                    cells[parseInt(j.toString(), 10)].style.left = (j === 0) ? '0px' : this.calculatedLeftValue(j - 1) + 'px';
                }
            }
        }
    }

    public calculatedLeftValue(index: number): number {
        const columns: Column[] = this.parent.columnModel;
        if (columns === null || columns.length === 0 || index < 0) {
            return 0;
        }
        let width: number = 0;
        const maxIndex: number = Math.min(index, columns.length - 1);
        for (let i: number = 0; i <= maxIndex; i++) {
            if (columns[parseInt(i.toString(), 10)] != null) {
                const columnWidth: string = columns[parseInt(i.toString(), 10)].width as string;
                width += parseInt(columnWidth.toString(), 10);
            }
        }
        return width;
    }
}
