import { describe, it, expect } from 'vitest';
import format from './format.helper';

describe('format.formatDate', () => {
    it('formats a valid date string to dd/mm/yyyy', () => {
        expect(format.formatDate('2024-01-05')).toBe('05/01/2024');
    });

    it('formats a Date object', () => {
        expect(format.formatDate(new Date(2024, 11, 25))).toBe('25/12/2024');
    });

    it('returns empty string for falsy input', () => {
        expect(format.formatDate(null)).toBe('');
        expect(format.formatDate('')).toBe('');
        expect(format.formatDate(undefined)).toBe('');
    });

    it('pads single-digit day and month with zero', () => {
        expect(format.formatDate('2024-03-07')).toBe('07/03/2024');
    });
});

describe('format.toDate', () => {
    it('converts dd/mm/yyyy string to Date object', () => {
        const result = format.toDate('05/01/2024');
        expect(result.getFullYear()).toBe(2024);
        expect(result.getMonth()).toBe(0); // January = 0
        expect(result.getDate()).toBe(5);
    });
});

describe('format.DateTime', () => {
    it('returns empty strings for falsy input', () => {
        const result = format.DateTime(null);
        expect(result.date).toBe('');
        expect(result.time).toBe('');
    });

    it('returns correct date and time parts', () => {
        const result = format.DateTime(new Date(2024, 0, 5, 14, 30));
        expect(result.date).toBe('05/01/2024');
        expect(result.time).toBe('14:30');
        expect(result.date_format).toBe('2024/01/05');
        expect(result.dateTime).toBe('05/01/2024 14:30');
    });

    it('pads hours and minutes with zero', () => {
        const result = format.DateTime(new Date(2024, 0, 5, 8, 5));
        expect(result.time).toBe('08:05');
    });
});

describe('format.FormatCurrency', () => {
    it('formats integer with 2 decimal places', () => {
        expect(format.FormatCurrency(1000000)).toBe('1,000,000.00');
    });

    it('formats with custom decimal places', () => {
        expect(format.FormatCurrency(1500, 0)).toBe('1,500');
    });

    it('formats decimal number', () => {
        expect(format.FormatCurrency(9999.5, 1)).toBe('9,999.5');
    });

    it('returns empty string for null', () => {
        expect(format.FormatCurrency(null)).toBe('');
        expect(format.FormatCurrency(undefined)).toBe('');
    });

    it('handles zero', () => {
        expect(format.FormatCurrency(0)).toBe('0.00');
    });

    it('handles negative numbers', () => {
        expect(format.FormatCurrency(-1000, 0)).toBe('-1,000');
    });
});
