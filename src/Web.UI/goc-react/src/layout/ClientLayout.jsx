import React from 'react';

const ClientLayOut = ({ children }) => {
    return (
        <div className="client-layout">
            {/* Add your layout styling and structure for client-side pages */}
            <header>Client Header</header>
            <main>{children}</main>
            <footer>Client Footer</footer>
        </div>
    );
};

export default ClientLayOut;
