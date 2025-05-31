import React from 'react';

export interface FormattedTextProps {
  content: string;
  className?: string;
}

/**
 * Parses markdown-style formatting and returns JSX with proper formatting
 * Supports: **bold**, *italic*, ***bold italic***
 */
export const parseFormattedText = (text: string): React.ReactNode[] => {
  if (!text) return [];

  const parts: React.ReactNode[] = [];
  let currentIndex = 0;
  let keyIndex = 0;

  // Regex to match formatting patterns
  // Order matters: bold italic first, then bold, then italic
  const formatRegex = /(\*\*\*[^*]+\*\*\*|\*\*[^*]+\*\*|\*[^*]+\*)/g;
  let match;

  while ((match = formatRegex.exec(text)) !== null) {
    // Add text before the match
    if (match.index > currentIndex) {
      const beforeText = text.slice(currentIndex, match.index);
      if (beforeText) {
        parts.push(<span key={`text-${keyIndex++}`}>{beforeText}</span>);
      }
    }

    const fullMatch = match[0];
    
    // Determine formatting type and extract content
    if (fullMatch.startsWith('***') && fullMatch.endsWith('***')) {
      // Bold italic: ***text***
      const content = fullMatch.slice(3, -3);
      parts.push(
        <strong key={`bold-italic-${keyIndex++}`}>
          <em>{content}</em>
        </strong>
      );
    } else if (fullMatch.startsWith('**') && fullMatch.endsWith('**')) {
      // Bold: **text**
      const content = fullMatch.slice(2, -2);
      parts.push(
        <strong key={`bold-${keyIndex++}`}>{content}</strong>
      );
    } else if (fullMatch.startsWith('*') && fullMatch.endsWith('*')) {
      // Italic: *text*
      const content = fullMatch.slice(1, -1);
      parts.push(
        <em key={`italic-${keyIndex++}`}>{content}</em>
      );
    }

    currentIndex = match.index + fullMatch.length;
  }

  // Add remaining text after last match
  if (currentIndex < text.length) {
    const remainingText = text.slice(currentIndex);
    if (remainingText) {
      parts.push(<span key={`text-${keyIndex++}`}>{remainingText}</span>);
    }
  }

  // If no formatting found, return original text
  if (parts.length === 0) {
    return [<span key="text-0">{text}</span>];
  }

  return parts;
};

/**
 * Component that renders formatted text
 */
export const FormattedText: React.FC<FormattedTextProps> = ({ content, className = '' }) => {
  const formattedContent = parseFormattedText(content);
  
  return (
    <span className={className}>
      {formattedContent}
    </span>
  );
};

/**
 * Utility functions for text formatting in input
 */
export const formatText = {
  /**
   * Wraps selected text with bold formatting
   */
  bold: (text: string, selectionStart: number, selectionEnd: number): { text: string; newCursorPos: number } => {
    const selectedText = text.slice(selectionStart, selectionEnd);
    const beforeText = text.slice(0, selectionStart);
    const afterText = text.slice(selectionEnd);
    
    if (selectedText) {
      // Wrap selected text
      const newText = `${beforeText}**${selectedText}**${afterText}`;
      return { text: newText, newCursorPos: selectionEnd + 4 };
    } else {
      // Insert bold markers at cursor
      const newText = `${beforeText}****${afterText}`;
      return { text: newText, newCursorPos: selectionStart + 2 };
    }
  },

  /**
   * Wraps selected text with italic formatting
   */
  italic: (text: string, selectionStart: number, selectionEnd: number): { text: string; newCursorPos: number } => {
    const selectedText = text.slice(selectionStart, selectionEnd);
    const beforeText = text.slice(0, selectionStart);
    const afterText = text.slice(selectionEnd);
    
    if (selectedText) {
      // Wrap selected text
      const newText = `${beforeText}*${selectedText}*${afterText}`;
      return { text: newText, newCursorPos: selectionEnd + 2 };
    } else {
      // Insert italic markers at cursor
      const newText = `${beforeText}**${afterText}`;
      return { text: newText, newCursorPos: selectionStart + 1 };
    }
  },

  /**
   * Checks if text contains formatting
   */
  hasFormatting: (text: string): boolean => {
    return /(\*\*\*[^*]+\*\*\*|\*\*[^*]+\*\*|\*[^*]+\*)/.test(text);
  },

  /**
   * Removes all formatting from text
   */
  stripFormatting: (text: string): string => {
    return text.replace(/\*\*\*([^*]+)\*\*\*/g, '$1')
              .replace(/\*\*([^*]+)\*\*/g, '$1')
              .replace(/\*([^*]+)\*/g, '$1');
  }
};
