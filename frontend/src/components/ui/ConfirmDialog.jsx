import Modal from './Modal.jsx'
import Button from './Button.jsx'

export default function ConfirmDialog({ open, title, message, onCancel, onConfirm, loading }) {
  return (
    <Modal
      open={open}
      onClose={onCancel}
      title={title}
      footer={
        <>
          <Button variant="secondary" onClick={onCancel} disabled={loading}>
            Cancel
          </Button>
          <Button variant="danger" onClick={onConfirm} disabled={loading}>
            {loading ? 'Deleting…' : 'Delete'}
          </Button>
        </>
      }
    >
      <p className="text-sm text-steel-600">{message}</p>
    </Modal>
  )
}
