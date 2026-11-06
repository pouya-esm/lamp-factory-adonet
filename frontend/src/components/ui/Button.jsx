const variants = {
  primary: 'bg-amber-500 hover:bg-amber-600 text-steel-950',
  secondary: 'bg-white border border-panel-300 hover:bg-panel-100 text-steel-800',
  danger: 'bg-stop-500 hover:bg-stop-600 text-white',
  ghost: 'text-steel-600 hover:bg-panel-100'
}

export default function Button({ variant = 'primary', className = '', disabled, children, ...props }) {
  return (
    <button
      disabled={disabled}
      className={`inline-flex items-center gap-1.5 rounded-md px-3.5 py-2 text-sm font-medium
        transition-colors disabled:opacity-60 disabled:cursor-not-allowed ${variants[variant]} ${className}`}
      {...props}
    >
      {children}
    </button>
  )
}
