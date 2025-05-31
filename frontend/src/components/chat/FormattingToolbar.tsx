import React from 'react';

interface FormattingToolbarProps {
  onBold: () => void;
  onItalic: () => void;
  disabled?: boolean;
}

const FormattingToolbar: React.FC<FormattingToolbarProps> = ({
  onBold,
  onItalic,
  disabled = false
}) => {
  return (
    <div className="flex items-center space-x-1 px-2 py-1 bg-gray-50 border border-gray-200 rounded-lg">
      <button
        type="button"
        onClick={onBold}
        disabled={disabled}
        className="p-1.5 text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
        title="Bold (Ctrl+B)"
        aria-label="Bold formatting"
      >
        <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
          <path fillRule="evenodd" d="M5 4a1 1 0 011-1h3a3 3 0 110 6H6v2h3a3 3 0 110 6H6a1 1 0 01-1-1V4zm2 2v3h2a1 1 0 100-2H7zm0 5v3h3a1 1 0 100-2H7z" clipRule="evenodd" />
        </svg>
      </button>
      
      <button
        type="button"
        onClick={onItalic}
        disabled={disabled}
        className="p-1.5 text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
        title="Italic (Ctrl+I)"
        aria-label="Italic formatting"
      >
        <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
          <path fillRule="evenodd" d="M8 3a1 1 0 011-1h2a1 1 0 110 2H9.5L7.5 16H9a1 1 0 110 2H7a1 1 0 01-1-1h2a1 1 0 110-2h1.5L11.5 4H10a1 1 0 01-1-1z" clipRule="evenodd" />
        </svg>
      </button>
      
      <div className="w-px h-4 bg-gray-300 mx-1" />
      
      <div className="text-xs text-gray-500 px-1">
        <span className="font-mono">**bold**</span> <span className="font-mono">*italic*</span>
      </div>
    </div>
  );
};

export default FormattingToolbar;
