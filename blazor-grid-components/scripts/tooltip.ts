import { attributes, createElement, isNullOrUndefined, getUniqueID, Browser, EventHandler, KeyboardEventArgs } from '@syncfusion/ej2-base';
import { OffsetPosition, calculatePosition } from '@syncfusion/ej2-popups';
import { SfGrid } from './sf-grid-fn';
import { parentsUntil } from './util';
import { Column } from './interfaces';

export class CustomToolTip {

    public content: string;
    public toolTipElement: HTMLElement;
    private ctrlId: string;
    private prevElement: HTMLElement;
    private parent: SfGrid;

    constructor(parent: SfGrid) {
        this.parent = parent;
        if (this.isEllipsisTooltip()) {
            this.wireEvents();
        }
    }

    public wireEvents(): void {
        EventHandler.add(this.parent.getContent(), 'scroll', this.scrollHandler, this);
        EventHandler.add(this.parent.element, 'mousemove', this.mouseMoveHandler, this);
        EventHandler.add(this.parent.element, 'mouseout', this.mouseMoveHandler, this);
        EventHandler.add(this.parent.element, 'keydown', this.onKeyPressed, this);
    }

    private unWireevents(): void {
        EventHandler.remove(this.parent.getContent(), 'scroll', this.scrollHandler);
        EventHandler.remove(this.parent.element, 'mousemove', this.mouseMoveHandler);
        EventHandler.remove(this.parent.element, 'mouseout', this.mouseMoveHandler);
        EventHandler.remove(this.parent.element, 'keydown', this.onKeyPressed);
    }

    public updateEvents(): void {
        if (this.isEllipsisTooltip()) {
            this.unWireevents();
            this.wireEvents();
        } else {
            this.unWireevents();
        }
    }

    public open(target: HTMLElement): void {
        this.close();
        this.ctrlId = getUniqueID(this.parent.element.getAttribute('id'));
        if (isNullOrUndefined(this.toolTipElement)) {
            this.toolTipElement = createElement('div', {
                className: 'e-tooltip-wrap e-popup e-lib e-control e-popup-open',
                styles: 'width: "auto", height: "auto", position: "absolute"',
                attrs: { role: 'tooltip', 'aria-hidden': 'false', 'id': this.ctrlId + '_content' }
            });
        }
        attributes(target, { 'aria-describedby': this.ctrlId + '_content', 'data-tooltip-id': this.ctrlId + '_content' });
        this.renderToolTip();
        this.setPosition(target);
    }

    private renderToolTip(): void {
        const content: HTMLElement = createElement('div', { className: 'e-tip-content' });
        content.textContent = this.content;
        this.toolTipElement.appendChild(content);
        const arrow: HTMLElement = createElement('div', { className: 'e-arrow-tip e-tip-bottom', styles: 'top: 99.9%' });
        arrow.appendChild(createElement('div', { className: 'e-arrow-tip-outer e-tip-bottom' }));
        arrow.appendChild(createElement('div', { className: 'e-arrow-tip-inner e-tip-bottom', styles: 'top: -6px' }));
        this.toolTipElement.appendChild(arrow);
        document.body.appendChild(this.toolTipElement);
    }

    private setPosition(target: HTMLElement): void {
        const tooltipPostion: { left: number; top: number; } = { top: 0, left: 0 };
        const arrow: HTMLElement = this.toolTipElement.querySelector('.e-arrow-tip');
        const popUpPosition: OffsetPosition = calculatePosition(target, 'Center', 'Top');
        tooltipPostion.top -= this.toolTipElement.offsetHeight + arrow.offsetHeight;
        tooltipPostion.left -= this.toolTipElement.offsetWidth / 2;
        this.toolTipElement.style.top = popUpPosition.top + tooltipPostion.top + 'px';
        this.toolTipElement.style.left = popUpPosition.left + tooltipPostion.left + 'px';
        const dialogElement: Element = parentsUntil(this.parent.element, 'e-dialog') || parentsUntil(this.parent.element, 'e-multicolumn-list');
        if (dialogElement) {
            this.toolTipElement.style.zIndex = (parseInt((dialogElement as HTMLElement).style.zIndex, 10) + 1).toString();
        }
    }

    public close(): void {
        if (this.toolTipElement) {
            const prevTarget: HTMLElement = this.parent.element.querySelector('[aria-describedby="' + this.ctrlId + '_content' + '"]');
            if (!isNullOrUndefined(prevTarget)) {
                prevTarget.removeAttribute('aria-describedby');
                prevTarget.removeAttribute('data-tooltip-id');
                this.toolTipElement = null;
            } else if (!isNullOrUndefined(this.parent.element.querySelector('form'))) {
                if (!isNullOrUndefined(document.getElementById(this.ctrlId + '_content'))) {
                    document.getElementById(this.ctrlId + '_content').remove();
                }
                this.toolTipElement = null;
            }
            if (!isNullOrUndefined(document.getElementById(this.ctrlId + '_content'))) {
                document.getElementById(this.ctrlId + '_content').remove();
                this.toolTipElement = null;
            }
        }
    }

    private getTooltipStatus(element: HTMLElement): boolean {

        const headerTable: Element = this.parent.getHeaderTable();
        const contentTable: Element = this.parent.getContentTable();
        const headerDivTag: string = 'e-gridheader';
        const contentDivTag: string = 'e-gridcontent';
        const htable: HTMLDivElement = this.createTable(headerTable, headerDivTag, 'header');
        const ctable: HTMLDivElement = this.createTable(contentTable, contentDivTag, 'content');
        //let td: HTMLElement = element;
        const table: HTMLDivElement = element.classList.contains('e-headercell') ? htable : ctable;
        const ele: string = element.classList.contains('e-headercell') ? 'th' : 'tr';
        table.querySelector(ele).className = element.className;
        (table.querySelector(ele) as HTMLElement).innerText = element.innerText;
        const width: number = table.querySelector(ele).getBoundingClientRect().width;
        document.body.removeChild(htable);
        document.body.removeChild(ctable);
        if (element.firstElementChild) {
            const childElement: HTMLElement = element.firstElementChild as HTMLElement;
            if (childElement.offsetWidth === childElement.scrollWidth) {
                const clonedElement: HTMLElement = childElement.cloneNode(true) as HTMLElement;
                Object.assign(clonedElement.style, {
                    overflow: 'visible',
                    visibility: 'hidden',
                    width: 'fit-content',
                    position: 'absolute',
                    left: '-9999px'
                });
                element.appendChild(clonedElement);
                const clonedElementWidth: number = clonedElement.getBoundingClientRect().width;
                element.removeChild(clonedElement);
                return childElement.offsetWidth < clonedElementWidth;
            }
            return width > element.getBoundingClientRect().width || childElement.offsetWidth < childElement.scrollWidth;
        }
        else
        { return width > element.getBoundingClientRect().width || element.offsetWidth < element.scrollWidth;
        }
    }
    private mouseMoveHandler(e: MouseEvent): void {
        if (this.isEllipsisTooltip()) {
            if (this.parent.options.allowTextWrap) {
                const wrapMode: string = this.parent.options.wrapMode;
                const hideTooltip: boolean | Element = ((wrapMode === 'Header' && parentsUntil(e.target as Element, 'e-gridheader')) || (wrapMode === 'Content' && parentsUntil(e.target as Element, 'e-gridcontent')) || wrapMode === 'Both');
                if (hideTooltip) {
                    return;
                }
            }
            const element: HTMLElement = parentsUntil((e.target as Element), 'e-ellipsistooltip') as HTMLElement;
            if (this.prevElement !== element || e.type === 'mouseout') {
                this.close();
            }
            const tagName: string = (e.target as Element).tagName;
            const elemNames: string[] = ['A', 'BUTTON', 'INPUT'];
            if (element && e.type !== 'mouseout' && !(Browser.isDevice && elemNames.indexOf(tagName) !== -1)) {
                if (element.getAttribute('data-tooltip-id')) {
                    return;
                }
                if (this.getTooltipStatus(element)) {
                    if (element.getElementsByClassName('e-headertext').length) {
                        const column: Column = this.parent.getColumnByUid(element.querySelector('.e-headercelldiv').getAttribute('e-mappinguid'));
                        this.content = !isNullOrUndefined(column.description) ? column.description : (element.getElementsByClassName('e-headertext')[0] as HTMLElement).innerText;
                    } else {
                        this.content = element.innerText;
                    }
                    this.prevElement = element;
                    this.open(element);
                }
            }
        }
        this.hoverFrozenRows(e);
    }

    private hoverFrozenRows(e: MouseEvent): void {
        if (this.parent.options.frozenColumns) {
            const row: Element = parentsUntil(e.target as Element, 'e-row');
            const frozenHover: Element[] = [].slice.call(this.parent.element.querySelectorAll('.e-frozenhover'));
            if (frozenHover.length && e.type === 'mouseout') {
                for (let i: number = 0; i < frozenHover.length; i++) {
                    frozenHover[parseInt(i.toString(), 10)].classList.remove('e-frozenhover');
                }
            } else if (row) {
                const rows: Element[] = [].slice.call(this.parent.element.querySelectorAll('tr[aria-rowindex="' + (parseInt(row.getAttribute('aria-rowindex'), 10) - 1) + '"]'));
                rows.splice(rows.indexOf(row), 1);
                if (row.getAttribute('aria-selected') !== 'true') {
                    for (let i: number = 0; i < rows.length; i++) {
                        rows[parseInt(i.toString(), 10)].classList.add('e-frozenhover');
                    }
                } else {
                    for (let i: number = 0; i < rows.length; i++) {
                        rows[parseInt(i.toString(), 10)].classList.remove('e-frozenhover');
                    }
                }
            }
        }
    }

    private isEllipsisTooltip(): boolean {
        const cols: Column[] = this.parent.getColumns();
        if (this.parent.options.clipMode === 'EllipsisWithTooltip') {
            return true;
        }
        for (let i: number = 0; i < cols.length; i++) {
            if (cols[parseInt(i.toString(), 10)].clipMode === 'EllipsisWithTooltip') {
                return true;
            }
        }
        return false;
    }

    private scrollHandler(): void {
        if (this.isEllipsisTooltip()) {
            this.close();
        }
    }

    /**
     * To create table for ellipsiswithtooltip
     *
     * @param {Element} table - The table element to be used.
     * @param {string} tag - The tag name for the sub-div element.
     * @param {string} type - The type indicating whether it's 'header' or not.
     * @returns {HTMLDivElement} The created div element containing the table.
     *
     * @hidden
     */
    private createTable(table: Element, tag: string, type: string): HTMLDivElement {
        const myTableDiv: HTMLDivElement = createElement('div') as HTMLDivElement;
        myTableDiv.className = this.parent.element.className;
        myTableDiv.style.cssText = 'display: inline-block;visibility:hidden;position:absolute';
        const mySubDiv: HTMLDivElement = createElement('div') as HTMLDivElement;
        mySubDiv.className = tag;
        const myTable: HTMLTableElement = createElement('table') as HTMLTableElement;
        myTable.className = table.className;
        myTable.style.cssText = 'table-layout: auto;width: auto';
        const ele: string = (type === 'header') ? 'th' : 'td';
        const myTr: HTMLTableRowElement = createElement('tr') as HTMLTableRowElement;
        const mytd: HTMLElement = createElement(ele) as HTMLElement;
        myTr.appendChild(mytd);
        myTable.appendChild(myTr);
        mySubDiv.appendChild(myTable);
        myTableDiv.appendChild(mySubDiv);
        document.body.appendChild(myTableDiv);
        return myTableDiv;
    }

    private onKeyPressed(e: KeyboardEventArgs): void {
        if (e.key === 'Tab' || e.key === 'ShiftTab') {
            this.close();
        }
    }

    public destroy(): void {
        this.close();
        this.unWireevents();
    }
}
