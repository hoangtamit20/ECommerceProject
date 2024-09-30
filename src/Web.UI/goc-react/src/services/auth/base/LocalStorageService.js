export const LocalStorageService = {
    getToken: () => {
        return localStorage.getItem('Authentication-GOC-App-Token');
    },
    setToken: (token) => {
        localStorage.setItem('Authentication-GOC-App-Token', token);
    },
    removeToken: () => {
        localStorage.removeItem('Authentication-GOC-App-Token');
    },
    getPreviousUrl: () => {
        return localStorage.getItem('previousUrl');
    },
    setPreviousUrl: (url) => {
        localStorage.setItem('previousUrl', url);
    },
    removePreviousUrl: () => {
        localStorage.removeItem('previousUrl');
    }
};
