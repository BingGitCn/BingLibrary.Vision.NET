using HalconDotNet;

namespace BingLibrary.Vision
{
    public class ROILine : ROIBase
    {
        private double row1, col1;   // first end point of line
        private double row2, col2;   // second end point of line
        private double midR, midC;   // midPoint of line

        private HXLDCont arrowHandleXLD;

        public ROILine()
        {
            numHandles = 3;        // two end points of line
            activeHandleIdx = 2;
            arrowHandleXLD = new HXLDCont();
            arrowHandleXLD.GenEmptyObj();
        }

        public override void createROILine(double row1, double col1, double row2, double col2)
        {
            midR = (row1 + row2) / 2;
            midC = (col1 + col2) / 2;

            this.row1 = row1;
            this.col1 = col1;
            this.row2 = row2;
            this.col2 = col2;

            updateArrowHandle();
        }

        public override void draw(HalconDotNet.HWindow window)
        {
            window.DispLine(row1, col1, row2, col2);

            window.DispRectangle2(row1, col1, 0, 15, 15);
            window.DispObj(arrowHandleXLD);  //window.DispRectangle2( row2, col2, 0, 5, 5);
            window.DispRectangle2(midR, midC, 0, 15, 15);
        }

        public override double distToClosestHandle(double x, double y)
        {
            double max = 10000;
            double[] val = new double[numHandles];

            val[0] = HMisc.DistancePp(y, x, row1, col1); // upper left
            val[1] = HMisc.DistancePp(y, x, row2, col2); // upper right
            val[2] = HMisc.DistancePp(y, x, midR, midC); // midpoint

            for (int i = 0; i < numHandles; i++)
            {
                if (val[i] < max)
                {
                    max = val[i];
                    activeHandleIdx = i;
                }
            }// end of for

            return val[activeHandleIdx];
        }

        public override void displayActive(HalconDotNet.HWindow window)
        {
            switch (activeHandleIdx)
            {
                case 0:
                    window.DispRectangle2(row1, col1, 0, 15, 15);
                    break;

                case 1:
                    window.DispObj(arrowHandleXLD); //window.DispRectangle2(row2, col2, 0, 5, 5);
                    break;

                case 2:
                    window.DispRectangle2(midR, midC, 0, 15, 15);
                    break;
            }
        }

        public override HRegion getRegion()
        {
            HRegion region = new HRegion();
            region.GenRegionLine(row1, col1, row2, col2);
            return region;
        }

        public override double getDistanceFromStartPoint(double row, double col)
        {
            double distance = HMisc.DistancePp(row, col, row1, col1);
            return distance;
        }

        public override HTuple getModelData()
        {
            return new HTuple(new double[] { row1, col1, row2, col2 });
        }

        public override void moveByHandle(double newX, double newY)
        {
            double lenR, lenC;

            switch (activeHandleIdx)
            {
                case 0: // first end point
                    row1 = newY;
                    col1 = newX;

                    midR = (row1 + row2) / 2;
                    midC = (col1 + col2) / 2;
                    break;

                case 1: // last end point
                    row2 = newY;
                    col2 = newX;

                    midR = (row1 + row2) / 2;
                    midC = (col1 + col2) / 2;
                    break;

                case 2: // midpoint
                    lenR = row1 - midR;
                    lenC = col1 - midC;

                    midR = newY;
                    midC = newX;

                    row1 = midR + lenR;
                    col1 = midC + lenC;
                    row2 = midR - lenR;
                    col2 = midC - lenC;
                    break;
            }
            updateArrowHandle();
        }

        private void updateArrowHandle()
        {
            double length, dr, dc, halfHW;
            double rrow1, ccol1, rowP1, colP1, rowP2, colP2;

            double headLength = 15;
            double headWidth = 15;

            arrowHandleXLD.Dispose();
            arrowHandleXLD.GenEmptyObj();

            rrow1 = row1 + (row2 - row1) * 0.8;
            ccol1 = col1 + (col2 - col1) * 0.8;

            length = HMisc.DistancePp(rrow1, ccol1, row2, col2);
            if (length == 0)
                length = -1;

            dr = (row2 - rrow1) / length;
            dc = (col2 - ccol1) / length;

            halfHW = headWidth / 2.0;
            rowP1 = rrow1 + (length - headLength) * dr + halfHW * dc;
            rowP2 = rrow1 + (length - headLength) * dr - halfHW * dc;
            colP1 = ccol1 + (length - headLength) * dc - halfHW * dr;
            colP2 = ccol1 + (length - headLength) * dc + halfHW * dr;

            if (length == -1)
                arrowHandleXLD.GenContourPolygonXld(rrow1, ccol1);
            else
                arrowHandleXLD.GenContourPolygonXld(new HTuple(new double[] { rrow1, row2, rowP1, row2, rowP2, row2 }),
                                                    new HTuple(new double[] { ccol1, col2, colP1, col2, colP2, col2 }));
        }
    }
}