import { describe, it, expect } from 'vitest';
import { Validator, checkLength, ValidateResult, PATTERN } from './validate';

describe('ValidateResult', () => {
    it('initializes with result=true and empty errors', () => {
        const vr = new ValidateResult();
        expect(vr.result).toBe(true);
        expect(vr.errors).toEqual({});
    });
});

describe('checkLength', () => {
    it('returns true when string length is within range', () => {
        expect(checkLength('hello', 3, 10)).toBe(true);
    });

    it('returns false when string is shorter than min', () => {
        expect(checkLength('hi', 5, 10)).toBe(false);
    });

    it('returns false when string exceeds max', () => {
        expect(checkLength('hello world', 0, 5)).toBe(false);
    });

    it('handles arrays', () => {
        expect(checkLength([1, 2, 3], 2, 5)).toBe(true);
        expect(checkLength([1], 2, 5)).toBe(false);
    });

    it('uses 0 as default min when undefined', () => {
        expect(checkLength('x', undefined, 10)).toBe(true);
    });
});

describe('Validator — required field', () => {
    it('fails when required field is empty string', () => {
        const result = Validator(
            { name: '' },
            { name: { validators: { required: true, name: 'Tên' } } }
        );
        expect(result.result).toBe(false);
        expect(result.errors).toHaveProperty('name');
    });

    it('passes when required field has value', () => {
        const result = Validator(
            { name: 'An Tú' },
            { name: { validators: { required: true, name: 'Tên' } } }
        );
        expect(result.result).toBe(true);
        expect(result.errors).not.toHaveProperty('name');
    });

    it('uses nullMessage when provided', () => {
        const result = Validator(
            { email: '' },
            { email: { validators: { required: true, name: 'Email', nullMessage: 'Email bắt buộc' } } }
        );
        expect(result.errors['email']).toBe('Email bắt buộc');
    });
});

describe('Validator — pattern validation', () => {
    it('fails when value does not match email pattern', () => {
        const result = Validator(
            { email: 'not-an-email' },
            { email: { validators: { pattern: PATTERN.email, name: 'Email', patternMessage: 'Email không hợp lệ' } } }
        );
        expect(result.result).toBe(false);
        expect(result.errors['email']).toBe('Email không hợp lệ');
    });

    it('passes when value matches email pattern', () => {
        const result = Validator(
            { email: 'test@example.com' },
            { email: { validators: { pattern: PATTERN.email, name: 'Email' } } }
        );
        expect(result.result).toBe(true);
    });

    it('passes password pattern validation for strong password', () => {
        const result = Validator(
            { password: 'Abcdef1@' },
            { password: { validators: { pattern: PATTERN.password, name: 'Mật khẩu' } } }
        );
        expect(result.result).toBe(true);
    });

    it('fails password pattern for weak password', () => {
        const result = Validator(
            { password: 'weakpassword' },
            { password: { validators: { pattern: PATTERN.password, name: 'Mật khẩu' } } }
        );
        expect(result.result).toBe(false);
    });
});

describe('Validator — no validators defined', () => {
    it('returns result=true when options is empty object', () => {
        const result = Validator({ name: 'Test' }, {});
        expect(result.result).toBe(true);
    });
});

describe('PATTERN constants', () => {
    it('PATTERN.phone matches valid VN phone', () => {
        // phoneRegex uses global flag — create fresh instance each test
        const regex = new RegExp(PATTERN.phone.source);
        expect(regex.test('0901234567')).toBe(true);
    });

    it('PATTERN.identityId matches 12-digit CCCD', () => {
        const regex = new RegExp(PATTERN.identityId.source);
        expect(regex.test('079204012345')).toBe(true);
        expect(regex.test('12345')).toBe(false);
    });
});
