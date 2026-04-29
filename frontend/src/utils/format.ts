import { format } from "date-fns";

export function fDate (date: string| Date): string {
    if(!date) return '';
    const _date = new Date(date);
    return format(_date, 'dd/MM/yyyy')
}