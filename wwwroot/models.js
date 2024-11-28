class Instance {
  constructor({ id, name, volume, maxVolume = 0, newVolume = 0, minVolume = 0, maxSpike = 0, axes, isConnected, errorMessage }) {
    if (id === undefined || name === undefined, maxVolume === undefined) {
      throw new Error("id, name, and maxVolume are required fields.");
    }

    this.id = id;
    this.name = name;
    this.volume = volume;
    this.newVolume = newVolume;
    this.maxVolume = maxVolume;
    this.minVolume = minVolume;
    this.maxSpike = maxSpike;
    this.axes = axes.map(a => {
      a.newValue = a.value;
      a.newFrequency = a.frequency;
      return a;
    });
    this.spike = new Spike();
    this.break = 5;
    this.paused = false;
    this.isConnected = isConnected;
    this.errorMessage = errorMessage;
  }

  updateFrom(otherInstance) {
    this.volume = otherInstance.volume;
    this.isConnected = otherInstance.isConnected;
    this.errorMessage = otherInstance.errorMessage;
    this.newVolume = otherInstance.newVolume;
    this.axes = this.axes.map(axe => {
      var other = otherInstance.axes.find(a => a.tCodeAxis == axe.tCodeAxis);

      axe.value = other.value;
      axe.frequency = other.frequency;

      return axe;
    });
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

export { Instance, Spike };
