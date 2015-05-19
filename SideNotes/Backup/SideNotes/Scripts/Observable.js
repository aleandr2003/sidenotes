function Observable() {
    this._observers = {};
}

Observable.prototype = {

    constructor: Observable,

    addObserver: function (type, observer) {
        if (typeof this._observers[type] == "undefined") {
            this._observers[type] = [];
        }
        this._observers[type].push(observer);
    },

    removeObserver: function (type, observer) {
        if (this._observers[type] instanceof Array) {
            var observers = this._observers[type];
            for (var i = 0, len = observers.length; i < len; i++) {
                if (observers[i] === observer) {
                    observers.splice(i, 1);
                    break;
                }
            }
        }
    },

    dispatch: function (event) {
        if (typeof event == "string") {
            event = { type: event };
        }
        if (!event.target) {
            event.target = this;
        }

        if (!event.type) { 
            throw new Error("No type property");
        }

        if (this._observers[event.type] instanceof Array) {
            var observers = this._observers[event.type];
            for (var i = 0, len = observers.length; i < len; i++) {
                observers[i].call(this, event);
            }
        }
    }
};