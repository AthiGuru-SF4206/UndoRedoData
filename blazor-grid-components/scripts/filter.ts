import { SfGrid } from './sf-grid-fn';
import { parentsUntil } from './util';
import { isNullOrUndefined, EventHandler, MouseEventArgs, closest, Browser } from '@syncfusion/ej2-base';
import { calculateRelativeBasedPosition } from '@syncfusion/ej2-popups';

/**
 * The `Filter` module is used to set the Filter Dialog position dynamically.
 */

export class Filter {
    private parent: SfGrid;

    constructor(parent: SfGrid) {
        this.parent = parent;
    }

    /**
     *
     * Get Filter Popup Position.
     *
     * @param {string} dlgID - The dialog ID.
     * @param {string} ColUid - The column UID.
     * @param {string} type - The type of filtering.
     * @param {boolean} isColumnMenu - Indicates if it's a column menu.
     * @returns {void}
     * @hidden
     */
    public filterPopupRender(dlgID: string, ColUid: string, type: string, isColumnMenu: boolean): any {
        const dlgelement: HTMLElement = this.parent.element.querySelector('#' + dlgID);
        let leftValue: any = 0;
        let topValue: any = 0;
        if (!isNullOrUndefined(dlgelement)) {
            if (isColumnMenu) {
                EventHandler.add(dlgelement, 'mousedown', this.mouseDownHandler, this);
                dlgelement.style.maxHeight = type === 'excel' ? '800px' : '350px';
                const element: Element = document.getElementsByClassName(`e-${this.parent.element.id}-column-menu`)[0].getElementsByTagName('ul')[0];
                const li: Element = isNullOrUndefined(element.querySelector('.' + 'e-icon-filter')) ? element.getElementsByClassName('e-menu-item e-focused')[0] : element.querySelector('.' + 'e-icon-filter').parentElement;
                const ul: HTMLElement = this.parent.element.querySelector('.' + 'e-filter-popup');
                const gridPos: ClientRect = this.parent.element.getBoundingClientRect();
                const liPos: ClientRect = li.getBoundingClientRect();
                let left: number = liPos.left - gridPos.left;
                let top: number = liPos.top - gridPos.top;
                const elementVisible: string = dlgelement.style.display;
                dlgelement.style.display = 'block';
                if (gridPos.height < top) {
                    top = top - ul.offsetHeight + liPos.height;
                }
                else if (gridPos.height < top + ul.offsetHeight) {
                    top = gridPos.height - ul.offsetHeight;
                }
                if (gridPos.height < ul.offsetHeight) {
                    top = ul.offsetHeight - gridPos.height;
                }
                if (window.innerHeight < ul.offsetHeight + top + gridPos.top) {
                    top = window.innerHeight - ul.offsetHeight - gridPos.top;
                }
                left += (this.parent.options.enableRtl ? -ul.offsetWidth : liPos.width);
                if (gridPos.width <= left + ul.offsetWidth) {
                    left -= liPos.width + ul.offsetWidth;
                }
                else if (left < 0) {
                    left += ul.offsetWidth + liPos.width;
                }
                dlgelement.style.display = elementVisible;
                leftValue = left.toString();
                topValue = top.toString();
            }
        }
        return [leftValue, topValue];
    }

    public mouseDownHandler(args: MouseEventArgs): void {
        if ((args && closest(args.target as Element, '.e-filter-popup')
        || (args.currentTarget && (args.currentTarget as Document).activeElement &&
            parentsUntil((args.currentTarget as Document).activeElement as Element, 'e-filter-popup'))
        || parentsUntil(args.target as Element, 'e-popup') ||
        (parentsUntil(args.target as Element, 'e-popup-wrapper'))) && !Browser.isDevice) {
            this.parent.dotNetRef.invokeMethodAsync('PreventColumnMenuClose', true);
        }
    }
}
