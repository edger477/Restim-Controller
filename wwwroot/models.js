class Device {
  constructor({ id, soundCard = '', process = '', name, volume, maxVolume = 0, maxPain = 0 }) {
    if (id === undefined || name === undefined || volume === undefined) {
      throw new Error("id, name, and volume are required fields.");
    }

    this.id = id;
    this.soundCard = soundCard;
    this.process = process;
    this.name = name;
    this.volume = volume;
    this.maxVolume = maxVolume;
    this.maxPain = maxPain;
    this.spike = new Spike();
    this.break = 5;
    this.paused = false;
  }

  // Additional methods can be added here if needed
}

class Spike {
  /**
   *
   */
  constructor() {
    this._intensity = 0.2;
    this._period = 0.7;
    this.repetitions = 5;
    this._size = 50;
  }

  get intensity() {
    return this._intensity;
  }

  set intensity(intensity) {
    this._intensity = parseFloat(intensity);
  }

  get period() {
    return this._period;
  }

  set period(period) {
    this._period = parseFloat(period);
  }

  get size() {
    return this._size;
  }
  set size(size) {
    this._size = parseInt(size);
  }


  get frequency() {
    return 1 / this.period;
  }
  set frequency(frequency) {
    this.period = 1 / frequency;
  }
}

export { Device, Spike };
