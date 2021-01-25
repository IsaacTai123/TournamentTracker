using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerLibrary
{
    // TODO - Wire up the CreatePrize for text files
    public class TextConnector : IDataConnection
    {
        public PrizeModel CreatePrize(PrizeModel model)
        {
            //this is just the sample code
            model.Id = 1;

            return model;
        }
    }
}
