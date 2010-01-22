// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Peter Forstmeier" email="peter.forstmeier@t-online.de"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Drawing;
using System.Drawing.Printing;
using ICSharpCode.Reports.Core.Interfaces;

namespace ICSharpCode.Reports.Core
{
	/// <summary>
	/// Description of AbstractDataRenderer.
	/// </summary>
	public class AbstractDataRenderer : AbstractRenderer
	{
		IDataManager dataManager;
		DataNavigator dataNavigator;
		
		public AbstractDataRenderer(IReportModel model,
		                            IDataManager dataManager,
		                            ReportDocument reportDocument,
		                            ILayouter layout):base(model,reportDocument,layout)
		                           
		{
			if (dataManager == null) {
				throw new ArgumentNullException("dataManager");
			}
			this.dataManager = dataManager;
		}
		
		#region overrides
		internal override void ReportBegin(object sender, PrintEventArgs pea)
		{
			base.ReportBegin(sender, pea);
		}
		
		
		internal override void ReportQueryPage(object sender, QueryPageSettingsEventArgs qpea)
		{
			base.ReportQueryPage(sender, qpea);
			base.SinglePage.IDataNavigator = this.dataNavigator;
		}
		
		
		protected override Point RenderSection(ReportPageEventArgs rpea)
		{
			return new System.Drawing.Point(0,0);
		}
		
		
		#endregion
		
		protected Point RenderItems (ReportPageEventArgs rpea) 
		{
			base.SinglePage.IDataNavigator = this.dataNavigator;
			base.CurrentRow = this.dataNavigator.CurrentRow;
			IContainerItem container = null;
			bool hasContainer = false;
			foreach (BaseReportItem item in this.CurrentSection.Items) {
				container = item as IContainerItem;
				if (container != null) {
					hasContainer = true;
					break;
				}
			}
			if (hasContainer) {
				return DoContainerControl(this.CurrentSection,container,rpea);
				
			} else {
				return base.RenderSection(rpea);
			}
			
		}
		
		
		private  Point DoContainerControl (BaseSection section,
		                                   IContainerItem container,
		                                   ReportPageEventArgs rpea)
		{
			Point drawPoint	= Point.Empty;
			if (section.Visible){
				
				//Always set section.size to it's original value
				section.Size = this.SectionBounds.DetailSectionRectangle.Size;
				
				PrintHelper.SetLayoutForRow(rpea.PrintPageEventArgs.Graphics,base.Layout,section);
				section.Render (rpea);
				foreach (BaseReportItem item in section.Items) {
					if (item.Parent == null) {
						item.Parent = section;
					}
					item.SectionOffset = section.SectionOffset;
					item.Render(rpea);
					drawPoint = new Point(item.Location.X,
					                      section.SectionOffset + section.Size.Height);
					rpea.LocationAfterDraw = new Point (rpea.LocationAfterDraw.X,section.SectionOffset + section.Size.Height);
					
				}
				if ((section.CanGrow == false)&& (section.CanShrink == false)) {
					return new Point(section.Location.X,section.Size.Height);
				}
				return drawPoint;
			}
			
			return drawPoint;
		}
		
		#region Properties
		
		protected IDataManager DataManager 
		{
			get {
				return dataManager;
			}
		}
		
		
		protected DataNavigator DataNavigator
		{
			get {
				return this.DataManager.GetNavigator;
			}
			set {this.dataNavigator = value;}
		}
		
		#endregion
		
		#region IDisposable
				
		protected override void Dispose(bool disposing)
		{
			try {
				if (disposing)
			{
//				if (this.dataManager != null) {
//					this.dataManager.Dispose();
//					this.dataManager = null;
//				}
				if (this.dataNavigator != null) {
					this.dataNavigator= null;
				}
			}
			} finally {
				base.Dispose(disposing);
			}
			
		}
		#endregion
	}
}


