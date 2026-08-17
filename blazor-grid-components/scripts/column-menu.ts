import { SfGrid } from './sf-grid-fn';
import { calculatePosition } from '@syncfusion/ej2-popups';
import { closest, Browser, EventHandler, isNullOrUndefined } from '@syncfusion/ej2-base';

/**
 * The `ColumnMenu` module is used to show or hide columns dynamically.
 */
export class ColumnMenu {
    private parent: SfGrid;
    private key: string = null;
    private uid: string = null;

    constructor(parent: SfGrid) {
        this.parent = parent;
    }

    /**
     * Get columnMenu Position.
     *
     * @param {string} uid - The UID of the columnMenu
     * @param {boolean} isFilter - Whether the columnMenu is used for filtering.
     * @param {string} key - The key associated with the columnMenu.
     * @returns {void}
     * @hidden
     */
    public renderColumnMenu(uid: string, isFilter: boolean, key: string): object {
        this.key = key;
        this.uid = uid;
        const e: HTMLElement = this.parent.getColumnHeaderByUid(uid).querySelector('.e-columnmenu');
        const columnMenuElement: Element = document.getElementsByClassName(`e-${this.parent.element.id}-column-menu`)[0];
        (columnMenuElement as HTMLElement).style.position = 'absolute';
        const element: HTMLElement = columnMenuElement.getElementsByTagName('ul')[0];
        if (!isNullOrUndefined(element)) {
            let pos: { left: number; top: number; } = { top: 0, left: 0 };
            element.style.visibility = 'hidden';
            (columnMenuElement as HTMLElement).style.display = 'block';
            const elePos: ClientRect = element.getBoundingClientRect();
            element.classList.add('e-transparent');
            element.style.visibility = '';
            (columnMenuElement as HTMLElement).style.display = '';
            const headerCell: Element = this.getHeaderCell(e);
            if (this.parent.options.enableRtl) {
                pos = calculatePosition(headerCell, 'left', 'bottom');
            }
            else {
                pos = calculatePosition(headerCell, 'right', 'bottom');
                pos.left -= elePos.width;
            }
            pos.left -= scrollX;
            pos.top -= scrollY;
            if (isFilter) {
                EventHandler.add(element, 'mouseover', this.appendFilter, this);
                EventHandler.add(element, 'keydown', this.appendFilter, this);
            }
            return { Left: Math.sign(pos.left) !== -1 ? Math.ceil(pos.left) : 0, Top: Math.ceil(pos.top) };
        } else {
            return { Left: 1, Top: 1 };
        }
    }

    public setPosition(): void {
        const columnMenuElement: Element = document.getElementsByClassName(`e-${this.parent.element.id}-column-menu`)[0];
        const element: HTMLElement = !isNullOrUndefined(columnMenuElement) ? columnMenuElement.getElementsByTagName('ul')[0] : null;
        if (!isNullOrUndefined(element) && !isNullOrUndefined(this.uid) && !isNullOrUndefined(this.parent.getColumnHeaderByUid(this.uid))
            && !Browser.isDevice) {
            const e: HTMLElement = this.parent.getColumnHeaderByUid(this.uid).querySelector('.e-columnmenu');
            const headerCell: Element = this.getHeaderCell(e);
            const btnOffset: ClientRect = headerCell.getBoundingClientRect();
            let left: number = btnOffset.left + scrollX;
            let top: number = btnOffset.bottom + scrollY;
            const popupOffset: ClientRect = element.getBoundingClientRect();
            const docElement: HTMLElement = document.documentElement;
            if (btnOffset.bottom + popupOffset.height > docElement.clientHeight) {
                if (top - btnOffset.height - popupOffset.height > docElement.clientTop) {
                    top = top - btnOffset.height - popupOffset.height;
                }
            }
            if (btnOffset.left + popupOffset.width > docElement.clientWidth) {
                if (btnOffset.right - popupOffset.width > docElement.clientLeft) {
                    left = (left + btnOffset.width) - popupOffset.width;
                }
            }
            left = left - element.getBoundingClientRect().width + btnOffset.width;
            (columnMenuElement as HTMLElement).style.left = Math.ceil(left + 1) + 'px';
            (columnMenuElement as HTMLElement).style.top = Math.ceil(top + 1) + 'px';
        }
    }

    private appendFilter(e: Event): void {
        let showdialog: boolean = false;
        setTimeout(() => {
            if ((closest(e.target as Element, '#' + this.key)) && (this.parent.element.querySelector('.e-filter-popup') || !this.parent.element.querySelector('.e-filter-popup'))
                || !isNullOrUndefined((e.target as Element).parentElement) && (e.target as Element).parentElement.id === this.key && (this.parent.element.querySelector('.e-filter-popup') || !this.parent.element.querySelector('.e-filter-popup'))) {
                showdialog = true;
            }
            else if (!closest(e.target as Element, '#' + this.key) && this.parent.element.querySelector('.e-filter-popup')) {
                showdialog = false;
            }
            this.parent.dotNetRef.invokeMethodAsync('FilterMouseOverHandler', this.uid, showdialog);
        }, 10);
    }

    public getHeaderCell(e: Element): Element {
        return closest(e, 'th.e-headercell');
    }
}
