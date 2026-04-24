import { motion, AnimatePresence } from "framer-motion"

function Toast({ message, type, show }) {

  const bg =
    type === "success"
      ? "bg-green-600"
      : type === "error"
      ? "bg-red-600"
      : "bg-slate-700"

  return (
    <AnimatePresence>
      {show && (
        <motion.div
          initial={{ opacity: 0, y: -40 }}
          animate={{ opacity: 1, y: 0 }}
          exit={{ opacity: 0, y: -40 }}
          transition={{ duration: 0.3 }}
          className={`fixed top-6 right-6 px-6 py-3 rounded-lg shadow-lg text-white ${bg}`}
        >
          {message}
        </motion.div>
      )}
    </AnimatePresence>
  )
}

export default Toast
