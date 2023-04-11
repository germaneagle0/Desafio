namespace Distributor
{
    public abstract class Observable<TObserver>
        where TObserver: Observer
    {
        public List<TObserver> observers = new();
        public void Subscribe(TObserver obs) {
            observers.Add(obs);
        }
        abstract public void Notify();
    }


    public class ObservableStock: Observable<EmailObserver>
    {
        float min, max;
        Stock.B3 stock_manager;
        string stock_name;
        public string msg = "";
        enum State
        {
            SENT_DOWN,
            SENT_UP,
            READY
        }
        State current_state = State.READY;
        public ObservableStock(float min, float max, Stock.B3 stock_manager, string stock_name)
        {
            setRange(min, max);
            this.stock_manager = stock_manager;
            this.stock_name = stock_name;
        }

        public async Task<bool> Update()
        {
            float stockPrice = await getStockPrice();
            if(updateState(stockPrice) == State.READY)
            {
                UpdateMsg(stockPrice);
                Notify();
                return true;
            }
            return false;
        }

        public void setRange(float min, float max)
        {
            if (min < max)
            {
                this.min = min;
                this.max = max;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(min), $"The first argument should be less than the second argument.");
            }
        }

        private State updateState(float stockPrice)
        {
            if (current_state == State.SENT_DOWN) 
                if (stockPrice >= min) current_state = State.READY;
            if (current_state == State.SENT_UP) 
                if (stockPrice <= max) current_state = State.READY;
            if (current_state == State.READY)
            {
                if (stockPrice >= max)
                {
                    current_state = State.SENT_UP;
                }
                if (stockPrice <= min)
                {
                    current_state = State.SENT_DOWN;
                }
                return State.READY;
            }
            return current_state;
        }

        public override void Notify()
        {
            observers.ForEach(observer =>
            {
                observer.update(this.msg);
            });
        }

        private string UpdateMsg(float stockPrice)
        {
            if (stockPrice < max && stockPrice > min) this.msg = "";
            if (stockPrice < min) this.msg = $"Buy {this.stock_name}!";
            if (stockPrice > max) this.msg = $"Sell {this.stock_name}!";
            return this.msg;
        }

        public async Task<float> getStockPrice()
        {
            return await stock_manager.getStockPrice(this.stock_name);
        }
    } 

    public abstract class Observer
    {
        public abstract void update(string msg);
    }

    public class EmailObserver : Observer
    {
        Email.EmailSender emailSender;
        string emailTo;
        public EmailObserver(Email.EmailSender emailSender, string emailTo) { this.emailSender = emailSender; this.emailTo = emailTo; }

        public override void update(string msg)
        {
            emailSender.send(msg, emailTo);
        }
    }
}
