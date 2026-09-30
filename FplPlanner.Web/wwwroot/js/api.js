// Thin wrapper around fetch for the FPL Planner API.
// Errors come back as ProblemDetails; they are turned into readable messages here.
const api = (() => {
    async function request(method, url, body, isForm = false) {
        const options = { method, headers: {} };
        if (body !== undefined) {
            if (isForm) {
                options.body = body;
            } else {
                options.headers['Content-Type'] = 'application/json';
                options.body = JSON.stringify(body);
            }
        }

        const response = await fetch(url, options);
        const text = await response.text();
        const data = text ? JSON.parse(text) : null;

        if (!response.ok) {
            throw new Error(describeError(data, response.status));
        }
        return data;
    }

    function describeError(problem, status) {
        if (!problem) return `Грешка ${status}`;
        const fallback = problem.success === false ? 'Excel фајлот има грешки:' : `Грешка ${status}`;
        const lines = [problem.detail || problem.title || fallback];

        // Squad rule violations: a list of strings. Validation errors: { field: [messages] }.
        // Excel import errors: [{ row, column, message }].
        const errors = problem.errors;
        if (Array.isArray(errors)) {
            errors.forEach(e => lines.push(typeof e === 'string' ? e : `Ред ${e.row}, ${e.column}: ${e.message}`));
        } else if (errors && typeof errors === 'object') {
            Object.values(errors).flat().forEach(e => lines.push(e));
        }
        return lines.join('\n');
    }

    return {
        get: url => request('GET', url),
        post: (url, body) => request('POST', url, body),
        postForm: (url, formData) => request('POST', url, formData, true),
    };
})();
