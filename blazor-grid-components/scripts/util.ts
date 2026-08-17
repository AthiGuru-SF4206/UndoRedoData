import { isNullOrUndefined, createElement, remove, classList } from '@syncfusion/ej2-base';
import { IPosition } from './interfaces';
import { SfGrid } from './sf-grid-fn';
/**
 * The function used to update DOM using requestAnimationFrame.
 *
 * @param {Function} updateFunction Function that contains the actual update action.
 * @param {Function} callBack Callback function to execute after the update.
 * @returns {void}
 * @hidden
 */
export function getUpdateUsingRaf(updateFunction: Function, callBack: Function): void {
    requestAnimationFrame(() => {
        try {
            callBack(null, updateFunction());
        } catch (e) {
            callBack(e);
        }
    });
}

/**
 * @param {HTMLElement} node - Defines the row
 * @param {number} width - Defines the width
 * @param {boolean} isRtl - Boolean property
 * @param {string} position - Defines the position
 * @returns {void}
 * @hidden
 */
export function applyStickyLeftRightPosition(node: HTMLElement, width: number, isRtl: boolean, position: string): void {
    if (node == null) {
        return;
    }
    if (position === 'Left') {
        if (isRtl) {
            (node as HTMLElement).style.right = width + 'px';
        } else {
            (node as HTMLElement).style.left = width + 'px';
        }
    }
    if (position === 'Right') {
        if (isRtl) {
            (node as HTMLElement).style.left = width + 'px';
        } else {
            (node as HTMLElement).style.right = width + 'px';
        }
    }
}

/** @hidden */
let scrollWidth: number = null;

/**
 * @hidden
 * Retrieves the width of the scrollbar on the current browser.
 *
 * @returns {number} The width of the scrollbar in pixels.
 */
export function getScrollBarWidth(): number {
    if (scrollWidth !== null) { return scrollWidth; }
    const divNode: HTMLDivElement = document.createElement('div');
    let value: number = 0;
    divNode.style.cssText = 'width:100px;height: 100px;overflow: scroll;position: absolute;top: -9999px;';
    document.body.appendChild(divNode);
    value = (divNode.offsetWidth - divNode.clientWidth) | 0;
    document.body.removeChild(divNode);
    return scrollWidth = value;
}

/**
 * Retrieves the cumulative height of previous and next siblings of a given element.
 *
 * @param {HTMLElement} element The element whose siblings' heights are to be calculated.
 * @returns {number} The cumulative height of previous and next siblings.
 */
export function getSiblingsHeight(element: HTMLElement): number {
    const previous: number = getHeightFromDirection(element, 'previous');
    const next: number = getHeightFromDirection(element, 'next');
    return previous + next;
}

/**
 * Retrieves the cumulative height of siblings in a specific direction until encountering specified classes.
 *
 * @param {HTMLElement} element The starting element to traverse from.
 * @param {string} direction The direction ('previous' or 'next') to traverse sibling elements.
 * @returns {number} The cumulative height of sibling elements with specified classes.
 */
export function getHeightFromDirection(element: HTMLElement, direction: string): number {
    let sibling: HTMLElement = element[direction + 'ElementSibling'];
    let result: number = 0;
    const classList: string[] = ['e-gridheader', 'e-gridfooter', 'e-groupdroparea', 'e-gridpager', 'e-toolbar'];

    while (sibling) {
        if (classList.some((value: string) => sibling.classList.contains(value))) {
            result += sibling.offsetHeight;
        }
        sibling = sibling[direction + 'ElementSibling'];
    }

    return result;
}

/**
 * @hidden
 * Traverses up the DOM tree from a given element until it finds a parent element that matches the selector.
 *
 * @param {Element} elem The starting element for traversal.
 * @param {string} selector The selector to match against parent elements.
 * @param {boolean} [isID=false] Optional flag indicating if the selector is an ID.
 * @returns {Element} The matching parent element, or null if none found.
 */
export function parentsUntil(elem: Element, selector: string, isID?: boolean): Element {
    let parent: Element = elem;
    while (parent) {
        if (isID ? parent.id === selector : parent.classList.contains(selector)) {
            break;
        }
        parent = parent.parentElement;
    }
    return parent;
}

/** @hidden */
// eslint-disable-next-line @typescript-eslint/no-namespace
export namespace Global {
    // eslint-disable-next-line prefer-const
    export let timer: Object = null;
}


/**
 * @hidden
 * Retrieves the index of an element within an array of elements.
 *
 * @param {Element} element The element to search for in the array.
 * @param {Element[]} elements The array of elements to search within.
 * @returns {number} The index of the element if found, otherwise -1.
 */
export function getElementIndex(element: Element, elements: Element[]): number {
    let index: number = -1;
    for (let i: number = 0, len: number = elements.length; i < len; i++) {
        if (elements[parseInt(i.toString(), 10)].isEqualNode(element)) {
            index = i;
            break;
        }
    }
    return index;
}

/**
 * @hidden
 * Checks if a value exists in an array and returns its index if found, otherwise returns -1.
 *
 * @param {Object} value The value to search for in the array.
 * @param {Object[]} collection The array to search within.
 * @returns {number} The index of the value if found, otherwise -1.
 */
export function inArray(value: Object, collection: Object[]): number {
    for (let i: number = 0, len: number = collection.length; i < len; i++) {
        if (collection[parseInt(i.toString(), 10)] === value) {
            return i;
        }
    }
    return -1;
}

/**
 * Retrieves the position (x, y coordinates) from a MouseEvent or TouchEvent.
 *
 * @param {MouseEvent | TouchEvent} e The MouseEvent or TouchEvent from which to retrieve the position.
 * @returns {IPosition} An object containing x and y coordinates.
 */
export function getPosition(e: MouseEvent | TouchEvent): IPosition {
    const position: IPosition = {} as IPosition;
    position.x = (isNullOrUndefined((e as MouseEvent).clientX) ? (e as TouchEvent).changedTouches[0].clientX :
        (e as MouseEvent).clientX);
    position.y = (isNullOrUndefined((e as MouseEvent).clientY) ? (e as TouchEvent).changedTouches[0].clientY :
        (e as MouseEvent).clientY);
    return position;
}

/**
 * Iterates over an array or object and applies a predicate function to each item.
 *
 * @param {Array|Object} collection The array or object to iterate over.
 * @param {Function} predicate The function to apply to each item.
 * @returns {Array} A new array with the results of the predicate function.
 */
export function iterateArrayOrObject<T, U>(collection: U[], predicate: (item: Object, index: number) => T): T[] {
    const result: T[] = [];
    for (let i: number = 0, len: number = collection.length; i < len; i++) {
        const pred: T = predicate(collection[parseInt(i.toString(), 10)], i);
        if (!isNullOrUndefined(pred)) {
            result.push(<T>pred);
        }
    }
    return result;
}

/**
 * Checks if an action should be prevented based on the presence of updated elements and the state of a dialog.
 *
 * @param {HTMLElement} element The HTML element to check.
 * @returns {boolean} Returns true if the action should be prevented, false otherwise.
 */
export function isActionPrevent(element: HTMLElement, editMode: string): boolean {
    const dlg: HTMLElement = element.querySelector('#' + element.id + 'EditConfirm') as HTMLElement;
    const updatedElements: NodeListOf<Element> = element.querySelectorAll('.e-updatedtd');
    return (!isNullOrUndefined(editMode) && editMode === 'Batch' && updatedElements.length > 0) ? false
    : (updatedElements.length > 0 && (dlg ? dlg.classList.contains('e-popup-close') : true));
}   

/**
 * Determines if the grid is in a group adaptive state.
 *
 * @param {SfGrid} grid - The grid instance to check.
 * @returns {boolean} - Returns `true` if the grid is group adaptive, otherwise `false`.
 *
 * @hidden
 */
export function isGroupAdaptive(grid: SfGrid): boolean {
    return (grid.options.enableVirtualization && grid.options.groupCount > 0 && (grid.options.offline || grid.options.url === ''));
}

/** @hidden */
let rowHeight: number;
/**
 * Get the row height by creating a temporary table inside the given element.
 *
 * @param {HTMLElement} [element] - The element to append the temporary table to.
 * @returns {number} - The calculated height of a table row.
 */
export function getRowHeight(element?: HTMLElement): number {
    if (rowHeight !== undefined) {
        return rowHeight;
    }
    const table: HTMLTableElement = <HTMLTableElement>createElement('table', { className: 'e-table', styles: 'visibility: hidden' });
    table.innerHTML = '<tr><td class="e-rowcell">A<td></tr>';
    element.appendChild(table);
    const rect: ClientRect = table.querySelector('td').getBoundingClientRect();
    element.removeChild(table);
    rowHeight = Math.ceil(rect.height);
    return rowHeight;
}

/**
 * Removes elements that match the given selector from the target element.
 *
 * @param {Element} target - The target element from which to remove the matching elements.
 * @param {string} selector - The CSS selector to identify the elements to be removed.
 * @returns {void}
 *
 * @hidden
 */
export function removeElement(target: Element, selector: string): void {
    const elements: HTMLElement[] = [].slice.call(target.querySelectorAll(selector));
    for (let i: number = 0; i < elements.length; i++) {
        remove(elements[parseInt(i.toString(), 10)]);
    }
}

/**
 * Adds or removes active classes to/from the given cells.
 *
 * @param {Element[]} cells - The list of cell elements to modify.
 * @param {boolean} add - A flag indicating whether to add or remove the classes.
 * @param {...string[]} args - The classes to add or remove.
 * @returns {void}
 *
 * @hidden
 */
export function addRemoveActiveClasses(cells: Element[], add: boolean, ...args: string[]): void {
    for (let i: number = 0, len: number = cells.length; i < len; i++) {
        if (add) {
            classList(cells[parseInt(i.toString(), 10)], [...args], []);
            cells[parseInt(i.toString(), 10)].setAttribute('aria-selected', 'true');
        } else {
            classList(cells[parseInt(i.toString(), 10)], [], [...args]);
            cells[parseInt(i.toString(), 10)].removeAttribute('aria-selected');
        }
    }
}
/**
 * Recursively finds the root parent grid element for a given element.
 *
 * @param {HTMLElement | null} element - The starting HTML element to find the parent grid.
 * @returns {HTMLElement | null} The root parent grid element or null if not found.
 */
export function getRootElement(element: HTMLElement | null): HTMLElement | null {
    if (!element) {
        return null;
    }
    const parentGrid: HTMLElement | null = parentsUntil(element, 'e-grid') as HTMLElement | null;
    if (!parentGrid) {
        return null;
    }

    return parentsUntil(parentGrid, 'e-detailrow') ? getRootElement(parentGrid.parentElement) : parentGrid;
}

/**
 * @hidden
 * Retrieves a cell element from the DOM given a row UID and column index.
 *
 * @param {HTMLElement} parentElement The root grid element containing the rows.
 * @param {string} rowUid The unique identifier (data-uid) of the target row.
 * @param {number} cellColIndex The column index (aria-colindex) of the target cell.
 * @returns {HTMLElement | null} The matching cell element if found, otherwise null.
 */
export function getCellByRowUidAndColIndex(parentElement: HTMLElement, rowUid: string, cellColIndex: number): HTMLElement | null {
    let cell: HTMLElement | null = null;
    const rows: NodeListOf<Element> = parentElement.querySelectorAll(`[data-uid="${rowUid}"]`) as NodeListOf<Element>;
    if (!isNullOrUndefined(rows) && rows.length > 0 && !isNullOrUndefined(parentElement)) {
        let row: HTMLElement | null = null;
        for (let i: number = 0; i < rows.length; i++) {
            const el: HTMLElement | null = rows[parseInt(i.toString(), 10)] as HTMLElement | null;
            if (!isNullOrUndefined(el) && parentsUntil(el, 'e-grid') === parentElement) {
                row = el;
                break;
            }
        }

        if (!isNullOrUndefined(row)) {
            cell = row.querySelector(`[aria-colindex="${cellColIndex}"]`) as HTMLElement | null;
        }
    }
    return cell;
}
