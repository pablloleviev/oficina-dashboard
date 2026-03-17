import '@testing-library/jest-dom'

// mock do alert
global.alert = () => {}

// mock do canvas (resolve Chart.js)
HTMLCanvasElement.prototype.getContext = () => {}